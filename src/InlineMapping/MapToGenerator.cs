using InlineMapping.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;

namespace InlineMapping
{
	[Generator]
	internal sealed class MapToGenerator
		: ISourceGenerator
	{
		private static (ImmutableArray<Diagnostic> diagnostics, string? name, SourceText? text) GenerateMapping(
			ITypeSymbol sourceType, AttributeData attributeData, ConfigurationValues configurationValues)
		{
			MappingInformation information = new(sourceType, attributeData);

			if (!information.Diagnostics.Any(_ => _.Severity == DiagnosticSeverity.Error))
			{
				var text = new MappingBuilder(information, configurationValues).Text;
				return (information.Diagnostics, $"{sourceType.Name}_To_{information.DestinationType.Name}_Map.g.cs", text);
			}

			return (information.Diagnostics, null, null);
		}

		public void Execute(GeneratorExecutionContext context)
		{
			if (context.SyntaxReceiver is MapToReceiver receiver)
			{
				var compilation = context.Compilation;
				var mapToAttributeSymbol = compilation.GetTypeByMetadataName(typeof(MapToAttribute).FullName);
				
				foreach (var candidateTypeNode in receiver.Candidates)
				{
					var model = compilation.GetSemanticModel(candidateTypeNode.SyntaxTree);
					if (model.GetDeclaredSymbol(candidateTypeNode) is ITypeSymbol candidateTypeSymbol)
					{
						foreach (var mappingAttribute in candidateTypeSymbol.GetAttributes().Where(
							_ => _.AttributeClass!.Equals(mapToAttributeSymbol, SymbolEqualityComparer.Default)))
						{
							ConfigurationValues configurationValues = new(context, candidateTypeNode.SyntaxTree);
							var (diagnostics, name, text) = MapToGenerator.GenerateMapping(
								candidateTypeSymbol, mappingAttribute, configurationValues);

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
			}
		}

		public void Initialize(GeneratorInitializationContext context) => 
			context.RegisterForSyntaxNotifications(() => new MapToReceiver());
	}
}