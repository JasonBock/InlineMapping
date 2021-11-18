/*
using InlineMapping.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace InlineMapping.Tests;

public static class MappingInformationTests
{
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
			Assert.That(information.Source.Name, Is.EqualTo("Source"));
			Assert.That(information.Destination.Name, Is.EqualTo("Destination"));
			Assert.That(information.Diagnostics.Length, Is.EqualTo(3));
			Assert.That(information.Diagnostics.Count(_ => _.Id == NoMatchDiagnostic.Id), Is.EqualTo(2));
			Assert.That(information.Diagnostics.Count(_ => _.Id == NoPropertyMapsFoundDiagnostic.Id), Is.EqualTo(1));
			Assert.That(information.Node, Is.Not.Null);
			Assert.That(information.PropertyNames.Length, Is.EqualTo(0));
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
*/