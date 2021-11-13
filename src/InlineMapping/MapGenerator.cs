using InlineMapping.Configuration;
using InlineMapping.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace InlineMapping;

// TODO: This needs to eventually be "MapGenerator"
// once I've made all the necessary changes
[Generator]
internal sealed class MapIncrementalGenerator
	: IIncrementalGenerator
{
   public void Initialize(IncrementalGeneratorInitializationContext context)
   {
	  static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token) =>
		  // We're looking for type declarations that have attributes on them,
		  // or assembly-level attributes
		  (node is TypeDeclarationSyntax typeNode && typeNode.AttributeLists.Count > 0) ||
			  (node is AttributeSyntax);

	  static SyntaxNode? TransformTargets(GeneratorSyntaxContext context, CancellationToken token)
	  {
		 // We only want to return types with our map attributes
		 // or the assembly-level map attribute.
		 var node = context.Node;
		 var model = context.SemanticModel;

		 if (node is TypeDeclarationSyntax)
		 {
			// Now we're checking to see if the type's attributes has any
			// [MapFrom] or [MapTo] in its list.
			var mapFromAttributeSymbol = model.Compilation.GetTypeByMetadataName(typeof(MapFromAttribute).FullName);
			var mapToAttributeSymbol = model.Compilation.GetTypeByMetadataName(typeof(MapToAttribute).FullName);
			var typeSymbol = model.GetDeclaredSymbol(node, token) as INamedTypeSymbol;

			if (typeSymbol is not null &&
				typeSymbol.GetAttributes().Any(_ =>
					SymbolEqualityComparer.Default.Equals(_.AttributeClass!, mapToAttributeSymbol) ||
					SymbolEqualityComparer.Default.Equals(_.AttributeClass!, mapFromAttributeSymbol)))
			{
			   return node;
			}
		 }
		 else
		 {
			// Now we're checking to see if this node, which is an AttributeSyntax,
			// is a [Map].
			var mapAttributeSymbol = model.Compilation.GetTypeByMetadataName(typeof(MapAttribute).FullName);
			var symbol = model.GetSymbolInfo(node, token).Symbol!.ContainingSymbol;

			if (SymbolEqualityComparer.Default.Equals(symbol, mapAttributeSymbol))
			{
			   return node;
			}
		 }

		 return null;
	  }

	  var provider = context.SyntaxProvider
		  .CreateSyntaxProvider(IsSyntaxTargetForGeneration, TransformTargets)
		  .Where(static _ => _ is not null);
	  var compilationNodes = context.CompilationProvider.Combine(provider.Collect());
	  var output = context.AnalyzerConfigOptionsProvider.Combine(compilationNodes);

	  context.RegisterSourceOutput(output,
		  (context, source) => CreateOutput(source.Right.Left, source.Right.Right, source.Left, context));
   }

   private static void CreateOutput(Compilation compilation, ImmutableArray<SyntaxNode?> nodes,
	   AnalyzerConfigOptionsProvider options, SourceProductionContext context)
   {
	  if (nodes.Length > 0)
	  {
		 foreach (var node in nodes.Distinct())
		 {
			var mappings = new Dictionary<(INamedTypeSymbol, INamedTypeSymbol), SyntaxNode>();

			// Get what used to be "Targets" from the receiver,
			// and then run what is essentially in GenerateMappings.
			foreach (var (source, destination, mappingContext) in
				MapIncrementalGenerator.GetTargets(node!, compilation))
			{
			   var symbolKey = (source, destination);
			   if (!mappings.ContainsKey(symbolKey))
			   {
				  mappings.Add((source, destination), node!);
				  var information = new MappingIncrementalInformation(
					  node!, source, destination, mappingContext, compilation);

				  if (information.Diagnostics.Any(_ => _.Severity == DiagnosticSeverity.Error))
				  {
					 // generate the source.
					 var configuration = new ConfigurationValues(options, node!.SyntaxTree);
					 var builder = new MappingBuilder(source, destination,
						 information.PropertyNames, mappingContext,
						 compilation, configuration);
					 context.AddSource($"{source.Name}_To_{destination.Name}_Map.g.cs", builder.Text);
				  }

				  foreach (var diagnostic in information.Diagnostics)
				  {
					 context.ReportDiagnostic(diagnostic);
				  }
			   }
			   else
			   {
				  context.ReportDiagnostic(DuplicatedAttributeDiagnostic.Create(node!, mappings[symbolKey]));
			   }
			}
		 }
	  }
   }

   private static IEnumerable<(INamedTypeSymbol source, INamedTypeSymbol destination, MappingContext context)> GetTargets(
	   SyntaxNode node, Compilation compilation)
   {
	  var model = compilation.GetSemanticModel(node.SyntaxTree);

	  if (node is TypeDeclarationSyntax)
	  {
		 var mapFromAttributeSymbol = model.Compilation.GetTypeByMetadataName(typeof(MapFromAttribute).FullName);
		 var mapToAttributeSymbol = model.Compilation.GetTypeByMetadataName(typeof(MapToAttribute).FullName);
		 var typeSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(node)!;

		 foreach (var typeAttribute in typeSymbol.GetAttributes())
		 {
			if (SymbolEqualityComparer.Default.Equals(typeAttribute.AttributeClass!, mapToAttributeSymbol))
			{
			   yield return (typeSymbol, (INamedTypeSymbol)typeAttribute.ConstructorArguments[0].Value!,
				   new MappingContext((ContainingNamespaceKind)typeAttribute.ConstructorArguments[1].Value!,
					   (MatchingPropertyTypeKind)typeAttribute.ConstructorArguments[2].Value!));
			}
			else if (SymbolEqualityComparer.Default.Equals(typeAttribute.AttributeClass!, mapFromAttributeSymbol))
			{
			   yield return ((INamedTypeSymbol)typeAttribute.ConstructorArguments[0].Value!, typeSymbol,
				   new MappingContext((ContainingNamespaceKind)typeAttribute.ConstructorArguments[1].Value!,
					   (MatchingPropertyTypeKind)typeAttribute.ConstructorArguments[2].Value!));
			}
		 }
	  }
	  else
	  {
		 var mapAttributeSymbol = model.Compilation.GetTypeByMetadataName(typeof(MapAttribute).FullName);
		 var symbol = model.GetSymbolInfo(node).Symbol!.ContainingSymbol;

		 if (SymbolEqualityComparer.Default.Equals(symbol, mapAttributeSymbol))
		 {
			var attributeData = model.Compilation.Assembly.GetAttributes().Single(
				_ => _.ApplicationSyntaxReference!.GetSyntax() == node);
			var sourceType = (INamedTypeSymbol)attributeData.ConstructorArguments[0].Value!;
			var destinationType = (INamedTypeSymbol)attributeData.ConstructorArguments[1].Value!;
			yield return (sourceType, destinationType,
				new MappingContext((ContainingNamespaceKind)attributeData.ConstructorArguments[2].Value!,
					(MatchingPropertyTypeKind)attributeData.ConstructorArguments[3].Value!));
		 }
	  }
   }
}

[Generator]
internal sealed class MapGenerator
	: ISourceGenerator
{
   private static ImmutableArray<(ImmutableArray<Diagnostic> diagnostics, string? name, SourceText? text)> GenerateMappings(
	   MapReceiver receiver, Compilation compilation, AnalyzerConfigOptionsProvider optionsProvider)
   {
	  var results = ImmutableArray.CreateBuilder<(ImmutableArray<Diagnostic> diagnostics, string? name, SourceText? text)>();
	  var information = new MappingInformation(receiver, compilation);

	  foreach (var mapPair in information.Maps)
	  {
		 if (!mapPair.Value.diagnostics.Any(_ => _.Severity == DiagnosticSeverity.Error))
		 {
			var text = new MappingBuilder(mapPair.Key.source, mapPair.Key.destination, mapPair.Value.propertyNames, mapPair.Value.context,
				compilation, new ConfigurationValues(optionsProvider, mapPair.Value.node.SyntaxTree)).Text;
			results.Add((mapPair.Value.diagnostics, $"{mapPair.Key.source.Name}_To_{mapPair.Key.destination.Name}_Map.g.cs", text));
		 }
		 else
		 {
			results.Add((mapPair.Value.diagnostics, null, null));
		 }
	  }

	  return results.ToImmutable();
   }

   public void Execute(GeneratorExecutionContext context)
   {
	  if (context.SyntaxContextReceiver is MapReceiver receiver)
	  {
		 var results = MapGenerator.GenerateMappings(receiver, context.Compilation, context.AnalyzerConfigOptions);

		 foreach (var (diagnostics, name, text) in results)
		 {
			foreach (var diagnostic in diagnostics)
			{
			   context.ReportDiagnostic(diagnostic);
			}

			if (name is not null && text is not null)
			{
			   context.AddSource(name, text);
			}
		 }
	  }
   }

   public void Initialize(GeneratorInitializationContext context) =>
	   context.RegisterForSyntaxNotifications(() => new MapReceiver());
}