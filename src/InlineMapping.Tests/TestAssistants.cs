using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InlineMapping.Tests
{
	using GeneratorTest = CSharpSourceGeneratorTest<MapGenerator, NUnitVerifier>;

	internal static class TestAssistants
	{
		internal static async Task RunAsync(string code, 
			IEnumerable<(Type, string, string)> generatedSources,
			IEnumerable<DiagnosticResult> expectedDiagnostics)
		{
			var test = new GeneratorTest
			{
				ReferenceAssemblies = ReferenceAssemblies.Net.Net50,
				//MarkupOptions = MarkupOptions.UseFirstDescriptor,
				TestState =
				{
					Sources = { code },
				},
			};

			foreach(var generatedSource in generatedSources)
			{
				test.TestState.GeneratedSources.Add(generatedSource);
			}

			test.TestState.AdditionalReferences.Add(typeof(MapGenerator).Assembly);
			test.TestState.ExpectedDiagnostics.AddRange(expectedDiagnostics);
			await test.RunAsync();
		}
	}
}