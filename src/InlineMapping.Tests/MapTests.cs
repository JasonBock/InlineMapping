using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System.Collections.Immutable;
using System;
using System.Linq;

namespace InlineMapping.Tests
{
	public static class MapTests
	{
		[Test]
		public static void MapToSelf()
		{
			var (diagnostics, output) = MapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Source))]

public class Source 
{ 
	public string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(0));
				Assert.That(output, Does.Not.Contain("namespace"));
				Assert.That(output, Does.Contain("using System;"));
				Assert.That(output, Does.Contain("public static Source MapToSource(this Source self) =>"));
				Assert.That(output, Does.Contain("self is null ? throw new ArgumentNullException(nameof(self)) :"));
				Assert.That(output, Does.Contain("Id = self.Id,"));
			});
		}

		[Test]
		public static void MapWithSourcePropertyHavingInternalGetter()
		{
			var (diagnostics, output) = MapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Source 
{ 
	internal string Id { get; set; }
}

public class Destination
{ 
	public string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(0));
				Assert.That(output, Does.Not.Contain("namespace"));
				Assert.That(output, Does.Contain("using System;"));
				Assert.That(output, Does.Contain("public static Destination MapToDestination(this Source self) =>"));
				Assert.That(output, Does.Contain("self is null ? throw new ArgumentNullException(nameof(self)) :"));
				Assert.That(output, Does.Contain("Id = self.Id,"));
			});
		}

		[Test]
		public static void MapWithDestinationPropertyHavingInternalSetter()
		{
			var (diagnostics, output) = MapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Source 
{ 
	public string Id { get; set; }
}

public class Destination
{ 
	internal string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(0));
				Assert.That(output, Does.Not.Contain("namespace"));
				Assert.That(output, Does.Contain("using System;"));
				Assert.That(output, Does.Contain("public static Destination MapToDestination(this Source self) =>"));
				Assert.That(output, Does.Contain("self is null ? throw new ArgumentNullException(nameof(self)) :"));
				Assert.That(output, Does.Contain("Id = self.Id,"));
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