using InlineMapping.Descriptors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.Globalization;

namespace InlineMapping.Tests.Diagnostics
{
	public static class NoArgumentConstructorDiagnosticTests
	{
		[Test]
		public static void Create()
		{
			var diagnostic = NoArgumentConstructorDiagnostic.Create(SyntaxFactory.ClassDeclaration("A"));

			Assert.Multiple(() =>
			{
				Assert.That(diagnostic.GetMessage(), Is.EqualTo(NoArgumentConstructorDiagnostic.Message));
				Assert.That(diagnostic.Descriptor.Title.ToString(CultureInfo.CurrentCulture), Is.EqualTo(NoArgumentConstructorDiagnostic.Title));
				Assert.That(diagnostic.Id, Is.EqualTo(NoArgumentConstructorDiagnostic.Id));
				Assert.That(diagnostic.Severity, Is.EqualTo(DiagnosticSeverity.Error));
				Assert.That(diagnostic.Descriptor.Category, Is.EqualTo(DescriptorConstants.Usage));
			});
		}
	}
}