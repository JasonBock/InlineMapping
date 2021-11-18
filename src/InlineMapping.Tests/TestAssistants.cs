using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace InlineMapping.Tests;

using GeneratorTest = CSharpSourceGeneratorTest<MapGenerator, NUnitVerifier>;
using GeneratorIncrementalTest = CSharpIncrementalSourceGeneratorVerifier<MapIncrementalGenerator>;

internal static class TestAssistants
{
	internal static async Task RunAsync(string code,
		IEnumerable<(Type, string, string)> generatedSources,
		IEnumerable<DiagnosticResult> expectedDiagnostics)
	{
		var test = new GeneratorTest
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

	internal static async Task RunIncrementalAsync(string code,
		IEnumerable<(Type, string, string)> generatedSources,
		IEnumerable<DiagnosticResult> expectedDiagnostics)
	{
		var test = new GeneratorIncrementalTest.Test
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

		test.TestState.AdditionalReferences.Add(typeof(MapIncrementalGenerator).Assembly);
		test.TestState.ExpectedDiagnostics.AddRange(expectedDiagnostics);
		await test.RunAsync().ConfigureAwait(false);
	}
}