using InlineMapping.Configuration;
using InlineMapping.Diagnostics;
using InlineMapping.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace InlineMapping;

[Generator]
internal sealed class MapGenerator
	: IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token) =>
			// We're looking for attributes with an expected name
			node is AttributeSyntax attributeNode &&
				(attributeNode.Name.ToString() == "Map" || attributeNode.Name.ToString() == "MapAttribute" ||
				attributeNode.Name.ToString() == "MapFrom" || attributeNode.Name.ToString() == "MapFromAttribute" ||
				attributeNode.Name.ToString() == "MapTo" || attributeNode.Name.ToString() == "MapToAttribute");

		static (SyntaxNode node, INamedTypeSymbol source, INamedTypeSymbol destination, MappingContext context)?
			TransformTargets(GeneratorSyntaxContext context, CancellationToken token)
		{
			// We only want to return types with our map attributes
			// or the assembly-level map attribute.
			var node = (AttributeSyntax)context.Node;
			var model = context.SemanticModel;

			// AttributeSyntax maps to a IMethodSymbol (you're basically calling a constructor
			// when you declare an attribute on a member).
			var symbol = model.GetSymbolInfo(node, token).Symbol as IMethodSymbol;

			if (symbol is not null)
			{
				// Now we're checking to see if the containing symbol of the method symbol
				// we just found is the same as either [MapFrom] or [MapTo].
				var mapFromAttributeSymbol = model.Compilation.GetTypeByMetadataName(typeof(MapFromAttribute).FullName);
				var mapToAttributeSymbol = model.Compilation.GetTypeByMetadataName(typeof(MapToAttribute).FullName);

				if (SymbolEqualityComparer.Default.Equals(symbol.ContainingSymbol, mapToAttributeSymbol))
				{
					var sourceNode = node.FindParent<TypeDeclarationSyntax>()!;
					var source = (INamedTypeSymbol)model.GetDeclaredSymbol(sourceNode, token)!;
					var attributeData = node.GetAttributeData(model);

					if (attributeData is not null)
					{
						return (node, source, (INamedTypeSymbol)attributeData.ConstructorArguments[0].Value!,
							new MappingContext((ContainingNamespaceKind)attributeData.ConstructorArguments[1].Value!,
								(MatchingPropertyTypeKind)attributeData.ConstructorArguments[2].Value!));
					}
				}
				else if (SymbolEqualityComparer.Default.Equals(symbol.ContainingSymbol, mapFromAttributeSymbol))
				{
					var destinationNode = node.FindParent<TypeDeclarationSyntax>()!;
					var destination = (INamedTypeSymbol)model.GetDeclaredSymbol(destinationNode, token)!;
					var attributeData = node.GetAttributeData(model);

					if (attributeData is not null)
					{
						return (node, (INamedTypeSymbol)attributeData.ConstructorArguments[0].Value!, destination,
							new MappingContext((ContainingNamespaceKind)attributeData.ConstructorArguments[1].Value!,
								(MatchingPropertyTypeKind)attributeData.ConstructorArguments[2].Value!));
					}
				}
			}
			else
			{
				// Now we're checking to see if the containing symbol of the method symbol
				// we just found is the same as [Map].
				var mapAttributeSymbol = model.Compilation.GetTypeByMetadataName(typeof(MapAttribute).FullName);

				if (SymbolEqualityComparer.Default.Equals(symbol, mapAttributeSymbol))
				{
					var attributeData = node.GetAttributeData(model);

					if (attributeData is not null)
					{
						var source = (INamedTypeSymbol)attributeData.ConstructorArguments[0].Value!;
						var destination = (INamedTypeSymbol)attributeData.ConstructorArguments[1].Value!;

						return (node, source, destination,
							new MappingContext((ContainingNamespaceKind)attributeData.ConstructorArguments[2].Value!,
								(MatchingPropertyTypeKind)attributeData.ConstructorArguments[3].Value!));
					}
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

	private static void CreateOutput(Compilation compilation,
		ImmutableArray<(SyntaxNode node, INamedTypeSymbol source, INamedTypeSymbol destination, MappingContext context)?> targets,
		AnalyzerConfigOptionsProvider options, SourceProductionContext context)
	{
		if (targets.Length > 0)
		{
			var mappings = new Dictionary<(INamedTypeSymbol, INamedTypeSymbol), SyntaxNode>();

			foreach (var (node, source, destination, mappingContext) in
				targets.Distinct().Cast<(SyntaxNode node, INamedTypeSymbol source, INamedTypeSymbol destination, MappingContext context)>())
			{
				var symbolKey = (source, destination);
				if (!mappings.ContainsKey(symbolKey))
				{
					mappings.Add(symbolKey, node!);
					var information = new MappingInformation(
						node!, source, destination, mappingContext, compilation);

					if (!information.Diagnostics.Any(_ => _.Severity == DiagnosticSeverity.Error))
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