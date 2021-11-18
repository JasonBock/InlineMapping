using Microsoft.CodeAnalysis.Testing;

namespace InlineMapping.Tests;

using GeneratorTest = CSharpIncrementalSourceGeneratorVerifier<MapGenerator>;

internal static class TestAssistants
{
	internal static async Task RunAsync(string code,
		IEnumerable<(Type, string, string)> generatedSources,
		IEnumerable<DiagnosticResult> expectedDiagnostics)
	{
		var test = new GeneratorTest.Test
		{
			ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
			TestState =
			{
				Sources = { code },
			},
		};

		foreach (var generatedSource in generatedSources)
		{
			test.TestState.GeneratedSources.Add(generatedSource);
		}

		test.TestState.AdditionalReferences.Add(typeof(MapGenerator).Assembly);
		test.TestState.ExpectedDiagnostics.AddRange(expectedDiagnostics);
		await test.RunAsync().ConfigureAwait(false);
	}
}