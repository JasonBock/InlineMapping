﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace InlineMapping.Tests;

public static class MapReceiverTests
{
   [TestCase("using InlineMapping; [MapFrom(typeof(Source))] public class Source { }")]
   [TestCase("using InlineMapping; [MapFromAttribute(typeof(Source))] public class Source { }")]
   [TestCase("using InlineMapping; [MapTo(typeof(Source))] public class Source { }")]
   [TestCase("using InlineMapping; [MapToAttribute(typeof(Source))] public class Source { }")]
   public static async Task FindTargetsWhenAttributeIsTargetingATypeAsync(string code)
   {
	  var classDeclaration = (await SyntaxFactory.ParseSyntaxTree(code)
		  .GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

	  var receiver = new MapReceiver();
	  var context = MapReceiverTests.GetContext(classDeclaration);
	  receiver.OnVisitSyntaxNode(context);

	  Assert.That(receiver.Targets.Count, Is.EqualTo(1));
   }

   [TestCase("using InlineMapping; [MapFrom(typeof(Source), containingNamespaceKind: ContainingNamespaceKind.Global, matchingPropertyTypeKind: MatchingPropertyTypeKind.Exact)] public class Destination { } public class Source { }")]
   [TestCase("using InlineMapping; [MapTo(typeof(Destination), containingNamespaceKind: ContainingNamespaceKind.Global, matchingPropertyTypeKind: MatchingPropertyTypeKind.Exact)] public class Source { } public class Destination { }")]
   public static async Task FindKindValuesInAttributesAsync(string code)
   {
	  var classDeclaration = (await SyntaxFactory.ParseSyntaxTree(code)
		  .GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

	  var receiver = new MapReceiver();
	  var context = MapReceiverTests.GetContext(classDeclaration);
	  receiver.OnVisitSyntaxNode(context);

	  var targetValues = receiver.Targets[0];

	  Assert.Multiple(() =>
	  {
		 Assert.That(targetValues.source.Name, Is.EqualTo("Source"), nameof(targetValues.source));
		 Assert.That(targetValues.destination.Name, Is.EqualTo("Destination"), nameof(targetValues.destination));
		 Assert.That(targetValues.context.ContainingNamespaceKind, Is.EqualTo(ContainingNamespaceKind.Global),
				   nameof(targetValues.context.ContainingNamespaceKind));
		 Assert.That(targetValues.context.MatchingPropertyTypeKind, Is.EqualTo(MatchingPropertyTypeKind.Exact),
				   nameof(targetValues.context.MatchingPropertyTypeKind));
	  });
   }

   [TestCase("using InlineMapping; [assembly: Map(typeof(Source), typeof(Destination), containingNamespaceKind: ContainingNamespaceKind.Global, matchingPropertyTypeKind: MatchingPropertyTypeKind.Exact)] public class Source { } public class Destination { }")]
   public static async Task FindKindValuesInAssemblyAttributeAsync(string code)
   {
	  var classDeclaration = (await SyntaxFactory.ParseSyntaxTree(code)
		  .GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<AttributeSyntax>().First();

	  var receiver = new MapReceiver();
	  var context = MapReceiverTests.GetContext(classDeclaration);
	  receiver.OnVisitSyntaxNode(context);

	  var targetValues = receiver.Targets[0];

	  Assert.Multiple(() =>
	  {
		 Assert.That(targetValues.source.Name, Is.EqualTo("Source"), nameof(targetValues.source));
		 Assert.That(targetValues.destination.Name, Is.EqualTo("Destination"), nameof(targetValues.destination));
		 Assert.That(targetValues.context.ContainingNamespaceKind, Is.EqualTo(ContainingNamespaceKind.Global),
				   nameof(targetValues.context.ContainingNamespaceKind));
		 Assert.That(targetValues.context.MatchingPropertyTypeKind, Is.EqualTo(MatchingPropertyTypeKind.Exact),
				   nameof(targetValues.context.MatchingPropertyTypeKind));
	  });
   }

   [TestCase("using InlineMapping; [assembly: Map(typeof(Source), typeof(Source))] public class Source { }")]
   [TestCase("using InlineMapping; [assembly: MapAttribute(typeof(Source), typeof(Source))] public class Source { }")]
   public static async Task FindCandidatesWhenAttributeIsTargetingAnAssemblyAsync(string code)
   {
	  var attributeDeclaration = (await SyntaxFactory.ParseSyntaxTree(code)
		  .GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<AttributeSyntax>().First();

	  var receiver = new MapReceiver();
	  var context = MapReceiverTests.GetContext(attributeDeclaration);
	  receiver.OnVisitSyntaxNode(context);

	  Assert.That(receiver.Targets.Count, Is.EqualTo(1));
   }

   [TestCase("[Dummy] public class Source { }")]
   [TestCase("public class Source { }")]
   public static async Task FindCandidatesWithNoMatchesAsync(string code)
   {
	  var classDeclaration = (await SyntaxFactory.ParseSyntaxTree(code)
		  .GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

	  var receiver = new MapReceiver();
	  var context = MapReceiverTests.GetContext(classDeclaration);
	  receiver.OnVisitSyntaxNode(context);

	  Assert.That(receiver.Targets.Count, Is.EqualTo(0));
   }

   private static GeneratorSyntaxContext GetContext(SyntaxNode node)
   {
	  var tree = node.SyntaxTree;
	  var references = AppDomain.CurrentDomain.GetAssemblies()
		  .Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
		  .Select(_ => MetadataReference.CreateFromFile(_.Location))
		  .Concat(new[] { MetadataReference.CreateFromFile(typeof(MapGenerator).Assembly.Location) });
	  var compilation = CSharpCompilation.Create("generator", new SyntaxTree[] { tree },
		  references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	  var model = compilation.GetSemanticModel(tree);

	  return GeneratorSyntaxContextFactory.Create(node, model);
   }
}