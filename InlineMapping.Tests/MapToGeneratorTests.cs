using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace InlineMapping.Tests
{
	public static class MapToGeneratorTests
	{
		[Test]
		public static void GenerateHappyPath()
		{
			var (diagnostics, output) = MapToGeneratorTests.GetGeneratedOutput(
@"using InlineMapping.Metadata;

public class Destination { }

[MapTo(typeof(Destination)]
public class Source { }");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.GreaterThan(0));
			});
		}

		private static (ImmutableArray<Diagnostic>, string) GetGeneratedOutput(string source)
		{
			var syntaxTree = CSharpSyntaxTree.ParseText(source);
			var references = AppDomain.CurrentDomain.GetAssemblies()
				.Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
				.Select(_ => MetadataReference.CreateFromFile(_.Location));
			var compilation = CSharpCompilation.Create("generator", new SyntaxTree[] { syntaxTree },
				references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
			var generator = new MapToGenerator();

			var driver = new CSharpGeneratorDriver(compilation.SyntaxTrees[0].Options, 
				ImmutableArray.Create<ISourceGenerator>(generator), default!, ImmutableArray<AdditionalText>.Empty);
			driver.RunFullGeneration(compilation, out var outputCompilation, out var diagnostics);

			return (diagnostics, outputCompilation.SyntaxTrees.Last().ToString());
		}
	}
}