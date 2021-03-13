using InlineMapping.Descriptors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.Globalization;

namespace InlineMapping.Tests.Diagnostics
{
	public static class DuplicatedAttributeDiagnosticTests
	{
		[Test]
		public static void Create()
		{
			var diagnostic = DuplicatedAttributeDiagnostic.Create(
				SyntaxFactory.ClassDeclaration("A"), SyntaxFactory.ClassDeclaration("B"));

			Assert.Multiple(() =>
			{
				Assert.That(diagnostic.GetMessage(), Is.EqualTo(DuplicatedAttributeDiagnostic.Message));
				Assert.That(diagnostic.Descriptor.Title.ToString(CultureInfo.CurrentCulture), Is.EqualTo(DuplicatedAttributeDiagnostic.Title));
				Assert.That(diagnostic.Id, Is.EqualTo(DuplicatedAttributeDiagnostic.Id));
				Assert.That(diagnostic.Severity, Is.EqualTo(DiagnosticSeverity.Warning));
				Assert.That(diagnostic.Descriptor.Category, Is.EqualTo(DescriptorConstants.Usage));
			});
		}
	}
}