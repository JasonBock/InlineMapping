using InlineMapping.Descriptors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System;
using System.Linq;

namespace InlineMapping.Tests
{
	public static class MappingInformationTests
	{
		[Test]
		public static void Create()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Source
{
	public int Id { get; set; }
}

public class Destination
{
	public int Id { get; set; }
}";
			var information = MappingInformationTests.GetInformation(code);

			Assert.Multiple(() =>
			{
				Assert.That(information.Maps.Count, Is.EqualTo(1));
				var (key, value) = information.Maps.First();
				Assert.That(key.source.Name, Is.EqualTo("Source"));
				Assert.That(key.destination.Name, Is.EqualTo("Destination"));
				Assert.That(value.diagnostics.Length, Is.EqualTo(0));
				Assert.That(value.node, Is.Not.Null);
				Assert.That(value.maps.Length, Is.EqualTo(1));
			});
		}

		[Test]
		public static void CreateWhenDuplicateMappingExists()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

[MapTo(typeof(Destination))]
public class Source
{
	public int Id { get; set; }
}

public class Destination
{
	public int Id { get; set; }
}";
			var information = MappingInformationTests.GetInformation(code);

			Assert.Multiple(() =>
			{
				Assert.That(information.Maps.Count, Is.EqualTo(1));
				var (key, value) = information.Maps.First();
				Assert.That(key.source.Name, Is.EqualTo("Source"));
				Assert.That(key.destination.Name, Is.EqualTo("Destination"));
				Assert.That(value.diagnostics.Length, Is.EqualTo(1));
				Assert.That(value.diagnostics[0].Id, Is.EqualTo(DuplicatedAttributeDiagnostic.Id));
				Assert.That(value.node, Is.Not.Null);
				Assert.That(value.maps.Length, Is.EqualTo(1));
			});
		}

		[Test]
		public static void CreateWhenConstructorWithArgumentsExist()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Source
{
	public int Id { get; set; }
}

public class Destination
{
	public Destination(int id) =>
		this.Id = id;

	public int Id { get; set; }
}";
			var information = MappingInformationTests.GetInformation(code);

			Assert.Multiple(() =>
			{
				Assert.That(information.Maps.Count, Is.EqualTo(1));
				var (key, value) = information.Maps.First();
				Assert.That(key.source.Name, Is.EqualTo("Source"));
				Assert.That(key.destination.Name, Is.EqualTo("Destination"));
				Assert.That(value.diagnostics.Length, Is.EqualTo(1));
				Assert.That(value.diagnostics[0].Id, Is.EqualTo(NoArgumentConstructorDiagnostic.Id));
				Assert.That(value.node, Is.Not.Null);
				Assert.That(value.maps.Length, Is.EqualTo(1));
			});
		}

		[Test]
		public static void CreateWhenSourcePropertyDoesNotMap()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Source
{
	public string Name { get; set; }
	public int Id { get; set; }
}

public class Destination
{
	public int Id { get; set; }
}";
			var information = MappingInformationTests.GetInformation(code);

			Assert.Multiple(() =>
			{
				Assert.That(information.Maps.Count, Is.EqualTo(1));
				var (key, value) = information.Maps.First();
				Assert.That(key.source.Name, Is.EqualTo("Source"));
				Assert.That(key.destination.Name, Is.EqualTo("Destination"));
				Assert.That(value.diagnostics.Length, Is.EqualTo(1));
				Assert.That(value.diagnostics[0].Id, Is.EqualTo(NoMatchDiagnostic.Id));
				Assert.That(value.node, Is.Not.Null);
				Assert.That(value.maps.Length, Is.EqualTo(1));
			});
		}

		[Test]
		public static void CreateWhenDestinationPropertyDoesNotMap()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Source
{
	public int Id { get; set; }
}

public class Destination
{
	public string Name { get; set; }
	public int Id { get; set; }
}";
			var information = MappingInformationTests.GetInformation(code);

			Assert.Multiple(() =>
			{
				Assert.That(information.Maps.Count, Is.EqualTo(1));
				var (key, value) = information.Maps.First();
				Assert.That(key.source.Name, Is.EqualTo("Source"));
				Assert.That(key.destination.Name, Is.EqualTo("Destination"));
				Assert.That(value.diagnostics.Length, Is.EqualTo(1));
				Assert.That(value.diagnostics[0].Id, Is.EqualTo(NoMatchDiagnostic.Id));
				Assert.That(value.node, Is.Not.Null);
				Assert.That(value.maps.Length, Is.EqualTo(1));
			});
		}

		[Test]
		public static void CreateWhenNoMapsExist()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Source
{
	public int Id { get; set; }
}

public class Destination
{
	public string Name { get; set; }
}";
			var information = MappingInformationTests.GetInformation(code);

			Assert.Multiple(() =>
			{
				Assert.That(information.Maps.Count, Is.EqualTo(1));
				var (key, value) = information.Maps.First();
				Assert.That(key.source.Name, Is.EqualTo("Source"));
				Assert.That(key.destination.Name, Is.EqualTo("Destination"));
				Assert.That(value.diagnostics.Length, Is.EqualTo(3));
				Assert.That(value.diagnostics.Count(_ => _.Id == NoMatchDiagnostic.Id), Is.EqualTo(2));
				Assert.That(value.diagnostics.Count(_ => _.Id == NoPropertyMapsFoundDiagnostic.Id), Is.EqualTo(1));
				Assert.That(value.node, Is.Not.Null);
				Assert.That(value.maps.Length, Is.EqualTo(0));
			});
		}

		private static MappingInformation GetInformation(string source)
		{
			var syntaxTree = CSharpSyntaxTree.ParseText(source);
			var references = AppDomain.CurrentDomain.GetAssemblies()
				.Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
				.Select(_ => MetadataReference.CreateFromFile(_.Location))
				.Concat(new[] { MetadataReference.CreateFromFile(typeof(MapGenerator).Assembly.Location) });
			var compilation = CSharpCompilation.Create("generator", new SyntaxTree[] { syntaxTree },
				references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
			var model = compilation.GetSemanticModel(syntaxTree);

			var receiver = new MapReceiver();
			
			foreach (var attributeSyntax in syntaxTree.GetRoot().DescendantNodes(_ => true).OfType<AttributeSyntax>())
			{
				var context = GeneratorSyntaxContextFactory.Create(attributeSyntax, model);
				receiver.OnVisitSyntaxNode(context);
			}

			foreach (var typeDeclarationSyntax in syntaxTree.GetRoot().DescendantNodes(_ => true).OfType<TypeDeclarationSyntax>())
			{
				var context = GeneratorSyntaxContextFactory.Create(typeDeclarationSyntax, model);
				receiver.OnVisitSyntaxNode(context);
			}

			return new(receiver, compilation);
		}
	}
}