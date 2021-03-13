using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System.Collections.Immutable;
using System;
using System.Linq;
using InlineMapping.Descriptors;

namespace InlineMapping.Tests
{
	public static class DuplicationTests
	{
		[Test]
		public static void DuplicateMapFromAndMapTo()
		{
			var (diagnostics, output) = DuplicationTests.GetGeneratedOutput(
@"using InlineMapping;

[MapFrom(typeof(Source))]
public class Destination 
{ 
	public string Id { get; set; }
}

[MapTo(typeof(Destination))]
public class Source 
{ 
	public string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(1));
				Assert.That(diagnostics[0].Id, Is.EqualTo(DuplicatedAttributeDiagnostic.Id));
				Assert.That(output.Length, Is.GreaterThan(0));
			});
		}

		[Test]
		public static void DuplicateMapFromAndMap()
		{
			var (diagnostics, output) = DuplicationTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

[MapFrom(typeof(Source))]
public class Destination 
{ 
	public string Id { get; set; }
}

public class Source 
{ 
	public string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(1));
				Assert.That(diagnostics[0].Id, Is.EqualTo(DuplicatedAttributeDiagnostic.Id));
				Assert.That(output.Length, Is.GreaterThan(0));
			});
		}

		[Test]
		public static void DuplicateMapToAndMap()
		{
			var (diagnostics, output) = DuplicationTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; set; }
}

[MapTo(typeof(Destination))]
public class Source 
{ 
	public string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(1));
				Assert.That(diagnostics[0].Id, Is.EqualTo(DuplicatedAttributeDiagnostic.Id));
				Assert.That(output.Length, Is.GreaterThan(0));
			});
		}

		private static (ImmutableArray<Diagnostic>, string) GetGeneratedOutput(string source)
		{
			var syntaxTree = CSharpSyntaxTree.ParseText(source);
			var references = AppDomain.CurrentDomain.GetAssemblies()
				.Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
				.Select(_ => MetadataReference.CreateFromFile(_.Location))
				.Concat(new[] { MetadataReference.CreateFromFile(typeof(MapGenerator).Assembly.Location) });
			var compilation = CSharpCompilation.Create("generator", new SyntaxTree[] { syntaxTree },
				references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

			var originalTreeCount = compilation.SyntaxTrees.Length;
			var generator = new MapGenerator();

			var driver = CSharpGeneratorDriver.Create(ImmutableArray.Create<ISourceGenerator>(generator));
			driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

			var trees = outputCompilation.SyntaxTrees.ToList();

			return (diagnostics, trees.Count != originalTreeCount ? trees[^1].ToString() : string.Empty);
		}
	}
}