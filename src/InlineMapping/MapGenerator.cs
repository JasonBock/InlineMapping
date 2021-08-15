using InlineMapping.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;

namespace InlineMapping
{
	[Generator]
	internal sealed class MapGenerator
		: ISourceGenerator
	{
		private static ImmutableArray<(ImmutableArray<Diagnostic> diagnostics, string? name, SourceText? text)> GenerateMappings(
			MapReceiver receiver, Compilation compilation, AnalyzerConfigOptionsProvider optionsProvider)
		{
			var results = ImmutableArray.CreateBuilder<(ImmutableArray<Diagnostic> diagnostics, string? name, SourceText? text)>();
			var information = new MappingInformation(receiver, compilation);

			foreach(var mapPair in information.Maps)
			{
				if(!mapPair.Value.diagnostics.Any(_ => _.Severity == DiagnosticSeverity.Error))
				{
					var context = new MappingContext(mapPair.Value.containingNamespaceKind, mapPair.Value.matchingPropertyTypeKind);
					var text = new MappingBuilder(mapPair.Key.source, mapPair.Key.destination, mapPair.Value.propertyNames, context,
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

				foreach(var (diagnostics, name, text) in results)
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
}