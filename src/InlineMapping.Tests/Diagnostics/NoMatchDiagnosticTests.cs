using InlineMapping.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Globalization;

namespace InlineMapping.Tests.Diagnostics;

public static class NoMatchDiagnosticTests
{
   [Test]
   public static void Create()
   {
	  var syntaxTree = CSharpSyntaxTree.ParseText("public class A { public int B { get; } }");
	  var typeSyntax = syntaxTree.GetRoot().DescendantNodes(_ => true).OfType<TypeDeclarationSyntax>().Single();
	  var references = AppDomain.CurrentDomain.GetAssemblies()
		  .Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
		  .Select(_ => MetadataReference.CreateFromFile(_.Location))
		  .Concat(new[] { MetadataReference.CreateFromFile(typeof(MapGenerator).Assembly.Location) });
	  var compilation = CSharpCompilation.Create("generator", new SyntaxTree[] { syntaxTree },
		  references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	  var model = compilation.GetSemanticModel(syntaxTree, true);

	  var typeSymbol = model.GetDeclaredSymbol(typeSyntax)!;
	  var propertySymbol = (typeSymbol.GetMembers("B")[0] as IPropertySymbol)!;

	  var diagnostic = NoMatchDiagnostic.Create(propertySymbol, "C", typeSymbol);

	  Assert.Multiple(() =>
	  {
		 Assert.That(diagnostic.GetMessage(), Is.EqualTo("A match for B on the C type A could not be found."));
		 Assert.That(diagnostic.Descriptor.Title.ToString(CultureInfo.CurrentCulture), Is.EqualTo(NoMatchDiagnostic.Title));
		 Assert.That(diagnostic.Id, Is.EqualTo(NoMatchDiagnostic.Id));
		 Assert.That(diagnostic.Severity, Is.EqualTo(DiagnosticSeverity.Info));
		 Assert.That(diagnostic.Descriptor.Category, Is.EqualTo(DescriptorConstants.Usage));
	  });
   }
}