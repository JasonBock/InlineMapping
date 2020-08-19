using InlineMapping.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InlineMapping
{
	[Generator]
	public sealed class MapToGenerator
		: ISourceGenerator
	{
		private static (string? name, SourceText? text) GenerateMapping(
			ITypeSymbol sourceType, AttributeData attributeData)
		{
			// TODO: Return an immutable list of diagnostics for all violated validations.
			// Also, change the implementation such that we find all of them, rather than
			// one and then bailing out.
			var destinationType = (INamedTypeSymbol)attributeData.ConstructorArguments[0].Value!;

			if(destinationType.Constructors.Any(_ => _.DeclaredAccessibility == Accessibility.Public && _.Parameters.Length == 0))
			{
				// TODO: If the destination property is non-nullable, then the source must be as well.
				// If it's nullable, it doesn't matter.
				// Or should it? Do we do exact match on nullability?
				// We should probably report a diagnostic in this case if we decide to be "strict" with the
				// nullable annotation.
				var destinationProperties = destinationType.GetMembers().OfType<IPropertySymbol>();
				var maps = new List<string>();

				foreach (var sourceProperty in sourceType.GetMembers().OfType<IPropertySymbol>().Where(_ => _.GetMethod is not null))
				{
					var destinationProperty = destinationProperties.FirstOrDefault(
						_ => _.Name == sourceProperty.Name &&
							_.SetMethod is not null &&
							_.Type.Equals(sourceProperty.Type, SymbolEqualityComparer.Default));

					if (destinationProperty is not null)
					{
						maps.Add($"\t\t\t\t\t{destinationProperty.Name} = self.{sourceProperty.Name},");
					}
				}

				if (maps.Count > 0)
				{
					// TODO: If the namespace is the same as one of the usings, or it's "in" one of them, 
					// we probably don't need to declare the namespace explicitly then.
					var usingStatements = new SortedSet<string>
					{
						"using System;"
					};

					if (!destinationType.ContainingNamespace.IsGlobalNamespace)
					{
						usingStatements.Add($"using {destinationType.ContainingNamespace.ToDisplayString()};");
					}

					var namespaceStartingStatement = !sourceType.ContainingNamespace.IsGlobalNamespace ?
						$"namespace {sourceType.ContainingNamespace.ToDisplayString()}{Environment.NewLine}{{{Environment.NewLine}" : string.Empty;
					var namespaceEndingStatement = !sourceType.ContainingNamespace.IsGlobalNamespace ?
						$"{Environment.NewLine}}}" : string.Empty;

					var performNullCheck = !sourceType.IsValueType ?
						$"\t\t\tself is null ? throw new ArgumentNullException(nameof(self)) :{Environment.NewLine}" : string.Empty;

					var text = SourceText.From(
@$"{string.Join(Environment.NewLine, usingStatements)}{Environment.NewLine}{Environment.NewLine}{namespaceStartingStatement}	public static partial class {sourceType.Name}MapToExtensions
	{{
		public static {destinationType.Name} MapTo{destinationType.Name}(this {sourceType.Name} self) =>
{performNullCheck}				new {destinationType.Name}
				{{
{string.Join(Environment.NewLine, maps)}
				}};
	}}{namespaceEndingStatement}", Encoding.UTF8);
					return ($"{sourceType.Name}_To_{destinationType.Name}_Map.g.cs", text);
				}
				else
				{
					// TODO: Return a diagnostic stating that no maps were found.
					return (null, null);
				}
			}
			else
			{
				// TODO: Return a diagnostic stating that the destination type 
				// doesn't have a public, no-argument constructor
				return (null, null);
			}
		}

		public void Execute(SourceGeneratorContext context)
		{
			if (context.SyntaxReceiver is MapToReceiver receiver)
			{
				var compilation = context.Compilation;
				var mapToSymbol = compilation.GetTypeByMetadataName(typeof(MapToAttribute).FullName);

				foreach (var candidateTypeNode in receiver.Candidates)
				{
					var model = compilation.GetSemanticModel(candidateTypeNode.SyntaxTree);
					var candidateTypeSymbol = model.GetDeclaredSymbol(candidateTypeNode) as ITypeSymbol;

					if (candidateTypeSymbol is not null)
					{
						foreach (var mappingAttribute in candidateTypeSymbol.GetAttributes().Where(
							_ => _.AttributeClass!.Equals(mapToSymbol, SymbolEqualityComparer.Default)))
						{
							var (name, text) = MapToGenerator.GenerateMapping(candidateTypeSymbol, mappingAttribute);

							if(name is not null && text is not null)
							{
								context.AddSource(name, text);
							}
						}
					}
				}
			}
		}

		public void Initialize(InitializationContext context) => context.RegisterForSyntaxNotifications(() => new MapToReceiver());
	}
}