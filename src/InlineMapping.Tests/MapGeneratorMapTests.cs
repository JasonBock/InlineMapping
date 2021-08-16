using InlineMapping.Descriptors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace InlineMapping.Tests
{
	public static class MapGeneratorMapTests
	{
		[Test]
		public static void GenerateWithClasses()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

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
				Assert.That(diagnostics.Length, Is.EqualTo(0));
				Assert.That(output, Does.Not.Contain("namespace"));
				Assert.That(output, Does.Contain("using System;"));
				Assert.That(output, Does.Contain("public static Destination MapToDestination(this Source self) =>"));
				Assert.That(output, Does.Contain("self is null ? throw new ArgumentNullException(nameof(self)) :"));
				Assert.That(output, Does.Contain("Id = self.Id,"));
			});
		}

		[Test]
		public static void GenerateWithStructs()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public struct Destination 
{ 
	public string Id { get; set; }
}

public struct Source 
{ 
	public string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(0));
				Assert.That(output, Does.Not.Contain("namespace"));
				Assert.That(output, Does.Not.Contain("using System;"));
				Assert.That(output, Does.Contain("public static Destination MapToDestination(this Source self) =>"));
				Assert.That(output, Does.Not.Contain("self is null ? throw new ArgumentNullException(nameof(self)) :"));
				Assert.That(output, Does.Contain("Id = self.Id,"));
			});
		}

		[Test]
		public static void GenerateWithRecords()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public record Destination 
{ 
	public string Id { get; init; }
}

public record Source 
{ 
	public string Id { get; init; }
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
		public static void GenerateWhenSourceIsInNamespaceAndDestinationIsNotInNamespace()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(SourceNamespace.Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; set; }
}

namespace SourceNamespace
{
	public class Source 
	{ 
		public string Id { get; set; }
	}
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(0));
				Assert.That(output, Does.Contain("namespace SourceNamespace"));
				Assert.That(output, Does.Contain("Id = self.Id,"));
			});
		}

		[Test]
		public static void GenerateWhenSourceIsNotInNamespaceAndDestinationIsInNamespace()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(DestinationNamespace.Destination))]

namespace DestinationNamespace
{
	public class Destination 
	{ 
		public string Id { get; set; }
	}
}

public class Source 
{ 
	public string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(0));
				Assert.That(output, Does.Contain("using DestinationNamespace;"));
				Assert.That(output, Does.Contain("Id = self.Id,"));
			});
		}

		[Test]
		public static void GenerateWhenDestinationIsInSourceNamespace()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(BaseNamespace.SubNamespace.Source), typeof(BaseNamespace.Destination))]

namespace BaseNamespace
{
	public class Destination 
	{ 
		public string Id { get; set; }
	}
}

namespace BaseNamespace.SubNamespace
{
	public class Source 
	{ 
		public string Id { get; set; }
	}
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(0));
				Assert.That(output, Does.Not.Contain("using BaseNamespace;"));
				Assert.That(output, Does.Contain("namespace BaseNamespace.SubNamespace"));
				Assert.That(output, Does.Contain("Id = self.Id,"));
			});
		}

		[Test]
		public static void GenerateWhenDestinationIsNotInSourceNamespace()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(SourceNamespace.Source), typeof(DestinationNamespace.Destination))]

namespace DestinationNamespace
{
	public class Destination 
	{ 
		public string Id { get; set; }
	}
}

namespace SourceNamespace
{
	public class Source 
	{ 
		public string Id { get; set; }
	}
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(0));
				Assert.That(output, Does.Contain("using DestinationNamespace;"));
				Assert.That(output, Does.Contain("namespace SourceNamespace"));
				Assert.That(output, Does.Contain("Id = self.Id,"));
			});
		}

		[Test]
		public static void GenerateWhenNoPropertiesExist()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination { }

public class Source { }");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(1));
				Assert.That(diagnostics[0].Id, Is.EqualTo(NoPropertyMapsFoundDiagnostic.Id));
				Assert.That(output, Is.EqualTo(string.Empty));
			});
		}

		[Test]
		public static void GenerateWhenSourcePropertyIsNotPublic()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; set; }
}

public class Source 
{ 
	private string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(2));
				Assert.That(() => diagnostics.Single(_ => _.Id == NoPropertyMapsFoundDiagnostic.Id), Throws.Nothing);
				var noMatchMessage = diagnostics.Single(_ => _.Id == NoMatchDiagnostic.Id).GetMessage();
				Assert.That(noMatchMessage, Contains.Substring("Id"));
				Assert.That(noMatchMessage, Contains.Substring("destination type Destination"));
				Assert.That(output, Is.EqualTo(string.Empty));
			});
		}

		[Test]
		public static void GenerateWhenDestinationPropertyIsNotPublic()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	private string Id { get; set; }
}

public class Source 
{ 
	public string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(2));
				Assert.That(() => diagnostics.Single(_ => _.Id == NoPropertyMapsFoundDiagnostic.Id), Throws.Nothing);
				var noMatchMessage = diagnostics.Single(_ => _.Id == NoMatchDiagnostic.Id).GetMessage();
				Assert.That(noMatchMessage, Contains.Substring("Id"));
				Assert.That(noMatchMessage, Contains.Substring("source type Source"));
				Assert.That(output, Is.EqualTo(string.Empty));
			});
		}

		[Test]
		public static void GenerateWhenSourceGetterIsNotPublic()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; set; }
}

public class Source 
{ 
	public string Id { private get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(2));
				Assert.That(() => diagnostics.Single(_ => _.Id == NoPropertyMapsFoundDiagnostic.Id), Throws.Nothing);
				var noMatchMessage = diagnostics.Single(_ => _.Id == NoMatchDiagnostic.Id).GetMessage();
				Assert.That(noMatchMessage, Contains.Substring("Id"));
				Assert.That(noMatchMessage, Contains.Substring("destination type Destination"));
				Assert.That(output, Is.EqualTo(string.Empty));
			});
		}

		[Test]
		public static void GenerateWhenDestinationSetterIsNotPublic()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; private set; }
}

public class Source 
{ 
	public string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(2));
				Assert.That(() => diagnostics.Single(_ => _.Id == NoPropertyMapsFoundDiagnostic.Id), Throws.Nothing);
				var noMatchMessage = diagnostics.Single(_ => _.Id == NoMatchDiagnostic.Id).GetMessage();
				Assert.That(noMatchMessage, Contains.Substring("Id"));
				Assert.That(noMatchMessage, Contains.Substring("source type Source"));
				Assert.That(output, Is.EqualTo(string.Empty));
			});
		}

		[Test]
		public static void GenerateWhenDestinationHasNoAccessibleConstructors()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{
	private Destination() { }

	public string Id { get; set; }
}

public class Source 
{ 
	public string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(1));
				Assert.That(() => diagnostics.Single(_ => _.Id == NoAccessibleConstructorsDiagnostic.Id), Throws.Nothing);
				Assert.That(output, Is.EqualTo(string.Empty));
			});
		}

		[Test]
		public static void GenerateWhenSourceDoesNotMapAllProperties()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; set; }
}

public class Source 
{ 
	public string Id { get; set; }
	public string Name { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(1));
				var noMatchMessage = diagnostics.Single(_ => _.Id == NoMatchDiagnostic.Id).GetMessage();
				Assert.That(noMatchMessage, Contains.Substring("Name"));
				Assert.That(noMatchMessage, Contains.Substring("source type Source"));
				Assert.That(output, Does.Not.Contain("namespace"));
				Assert.That(output, Does.Contain("using System;"));
				Assert.That(output, Does.Contain("public static Destination MapToDestination(this Source self) =>"));
				Assert.That(output, Does.Contain("self is null ? throw new ArgumentNullException(nameof(self)) :"));
				Assert.That(output, Does.Contain("Id = self.Id,"));
			});
		}

		[Test]
		public static void GenerateWhenDestinationDoesNotMapAllProperties()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; set; }
	public string Name { get; set; }
}

public class Source 
{ 
	public string Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(1));
				var noMatchMessage = diagnostics.Single(_ => _.Id == NoMatchDiagnostic.Id).GetMessage();
				Assert.That(noMatchMessage, Contains.Substring("Name"));
				Assert.That(noMatchMessage, Contains.Substring("destination type Destination"));
				Assert.That(output, Does.Not.Contain("namespace"));
				Assert.That(output, Does.Contain("using System;"));
				Assert.That(output, Does.Contain("public static Destination MapToDestination(this Source self) =>"));
				Assert.That(output, Does.Contain("self is null ? throw new ArgumentNullException(nameof(self)) :"));
				Assert.That(output, Does.Contain("Id = self.Id,"));
			});
		}

		[Test]
		public static void GenerateWhenPropertyTypesDoNotMatch()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; set; }
}

public class Source 
{ 
	public int Id { get; set; }
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(3));
				Assert.That(() => diagnostics.Single(_ => _.Id == NoPropertyMapsFoundDiagnostic.Id), Throws.Nothing);
				Assert.That(() => diagnostics.Single(_ => _.Id == NoMatchDiagnostic.Id && 
					_.GetMessage().Contains("source type Source", StringComparison.InvariantCulture)), Throws.Nothing);
				Assert.That(() => diagnostics.Single(_ => _.Id == NoMatchDiagnostic.Id &&
					_.GetMessage().Contains("destination type Destination", StringComparison.InvariantCulture)), Throws.Nothing);
				Assert.That(output, Is.EqualTo(string.Empty));
			});
		}

		[Test]
		public static void GenerateWhenDestinationNamespaceIsSelected()
		{
			var (diagnostics, output) = MapGeneratorMapTests.GetGeneratedOutput(
@"using InlineMapping;

[assembly: Map(typeof(SourceNamespace.Source), typeof(DestinationNamespace.Destination), ContainingNamespaceKind.Destination)]

namespace DestinationNamespace
{
	public class Destination 
	{ 
		public string Id { get; set; }
	}
}

namespace SourceNamespace
{
	public class Source 
	{ 
		public string Id { get; set; }
	}
}");

			Assert.Multiple(() =>
			{
				Assert.That(diagnostics.Length, Is.EqualTo(0));
				Assert.That(output, Does.Contain("namespace DestinationNamespace"));
				Assert.That(output, Does.Contain("using SourceNamespace;"));
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