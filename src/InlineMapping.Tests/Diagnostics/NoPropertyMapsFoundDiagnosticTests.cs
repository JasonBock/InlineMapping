using InlineMapping.Descriptors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.Globalization;

namespace InlineMapping.Tests.Diagnostics
{
	public static class NoPropertyMapsFoundDiagnosticTests
	{
		[Test]
		public static void Create()
		{
			var diagnostic = NoPropertyMapsFoundDiagnostic.Create(SyntaxFactory.ClassDeclaration("A"));

			Assert.Multiple(() =>
			{
				Assert.That(diagnostic.GetMessage(), Is.EqualTo(NoPropertyMapsFoundDiagnostic.Message));
				Assert.That(diagnostic.Descriptor.Title.ToString(CultureInfo.CurrentCulture), Is.EqualTo(NoPropertyMapsFoundDiagnostic.Title));
				Assert.That(diagnostic.Id, Is.EqualTo(NoPropertyMapsFoundDiagnostic.Id));
				Assert.That(diagnostic.Severity, Is.EqualTo(DiagnosticSeverity.Error));
				Assert.That(diagnostic.Descriptor.Category, Is.EqualTo(DescriptorConstants.Usage));
			});
		}
	}
}