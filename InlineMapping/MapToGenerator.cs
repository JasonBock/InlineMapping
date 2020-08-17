using InlineMapping.Extensions;
using InlineMapping.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
			SemanticModel model, ITypeSymbol sourceType, AttributeData attributeData)
		{
			var destinationType = (model.GetDeclaredSymbol(
				(attributeData.ConstructorArguments[0].Value as TypeOfExpressionSyntax)!.Type) as INamedTypeSymbol)!;

			if(destinationType.Constructors.Length == 0 ||
				destinationType.Constructors.Any(_ => _.DeclaredAccessibility == Accessibility.Public && _.Parameters.Length == 0))
			{
				var destinationProperties = destinationType.GetMembers().OfType<IPropertySymbol>();
				var maps = new List<string>();

				foreach (var sourceProperty in sourceType.GetMembers().OfType<IPropertySymbol>().Where(_ => _.GetMethod is not null))
				{
					var destinationProperty = destinationProperties.FirstOrDefault(
						_ => _.Name == sourceProperty.Name &&
							_.SetMethod is not null &&
							_.Type.IsAssignableFrom(sourceProperty.Type));

					if (destinationProperty is not null)
					{
						maps.Add($"\t\t\t\t{destinationProperty.Name} = self.{sourceProperty.Name},");
					}
				}

				if (maps.Count > 0)
				{
					var text = SourceText.From(
@$"using {destinationType.ContainingNamespace.ToDisplayString()};

namespace {sourceType.ContainingNamespace.ToDisplayString()}
{{
	public static partial class {sourceType.Name}MapToExtensions
	{{
		public static {destinationType.Name} MapTo{destinationType.Name}(this {sourceType.Name} self) =>
			new {destinationType.Name}
			{{
{string.Join(Environment.NewLine, maps)}
			}};
	}}
}}", Encoding.UTF8);
					return ($"{sourceType.Name}_To_{destinationType.Name}_Map.g.cs", text);
				}
				else
				{
					return (null, null);
				}
			}
			else
			{
				return (null, null);
			}
		}

		public void Execute(SourceGeneratorContext context)
		{
			if (context.SyntaxReceiver is MapToReceiver receiver)
			{
				var compilation = context.Compilation;
				var mapToSymbol = compilation.GetTypeByMetadataName(typeof(MapToAttribute).FullName);

				foreach (var candidateClassNode in receiver.Candidates)
				{
					var model = compilation.GetSemanticModel(candidateClassNode.SyntaxTree);
					var candidateClassSymbol = model.GetDeclaredSymbol(candidateClassNode) as ITypeSymbol;

					if (candidateClassSymbol is not null)
					{
						foreach (var mappingAttribute in candidateClassSymbol.GetAttributes().Where(
							_ => _.AttributeClass!.Equals(mapToSymbol, SymbolEqualityComparer.Default)))
						{
							var (name, text) = MapToGenerator.GenerateMapping(model, candidateClassSymbol, mappingAttribute);

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