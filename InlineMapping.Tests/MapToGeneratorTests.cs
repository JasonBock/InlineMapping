using InlineMapping.Metadata;
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
		public static void Generate()
		{
			var (diagnostics, output) = MapToGeneratorTests.GetGeneratedOutput(
@"using InlineMapping.Metadata;

public class Destination 
{ 
	public string Id { get; set; }
}

[MapTo(typeof(Destination)]
public class Source 
{ 
	public string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(0));
				Assert.That(output, Does.Not.Contain("namespace"));
				Assert.That(output, Does.Contain("public static Destination MapToDestination(this Source self) =>"));
				Assert.That(output, Does.Contain("self is null ? throw new ArgumentNullException(nameof(self)) :"));
				Assert.That(output, Does.Contain("Id = self.Id,"));
			});
		}

		[Test]
		public static void GenerateWhenNoPropertiesExist()
		{
			var (diagnostics, output) = MapToGeneratorTests.GetGeneratedOutput(
@"using InlineMapping.Metadata;

public class Destination { }

[MapTo(typeof(Destination)]
public class Source { }");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(1));
				Assert.That(diagnostics[0].Id, Is.EqualTo("IM0002"));
				Assert.That(output, Is.EqualTo(string.Empty));
			});
		}

		[Test]
		public static void GenerateWhenDestinationPropertyIsNotPublic()
		{
			var (diagnostics, output) = MapToGeneratorTests.GetGeneratedOutput(
@"using InlineMapping.Metadata;

public class Destination 
{ 
	internal string Id { get; set; }
}

[MapTo(typeof(Destination)]
public class Source 
{ 
	public string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(1));
				Assert.That(diagnostics[0].Id, Is.EqualTo("IM0002"));
				Assert.That(output, Is.EqualTo(string.Empty));
			});
		}

		private static (ImmutableArray<Diagnostic>, string) GetGeneratedOutput(string source)
		{
			var syntaxTree = CSharpSyntaxTree.ParseText(source);
			var references = AppDomain.CurrentDomain.GetAssemblies()
				.Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
				.Select(_ => MetadataReference.CreateFromFile(_.Location))
				.Concat(new[] { MetadataReference.CreateFromFile(typeof(MapToAttribute).Assembly.Location) });
			var compilation = CSharpCompilation.Create("generator", new SyntaxTree[] { syntaxTree },
				references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
			var generator = new MapToGenerator();

			var driver = new CSharpGeneratorDriver(compilation.SyntaxTrees[0].Options,
				ImmutableArray.Create<ISourceGenerator>(generator), default!, ImmutableArray<AdditionalText>.Empty);
			driver.RunFullGeneration(compilation, out var outputCompilation, out var diagnostics);

			var trees = outputCompilation.SyntaxTrees.ToList();

			return (diagnostics, trees.Count == 2 ? trees[^1].ToString() : string.Empty);
		}
	}
}