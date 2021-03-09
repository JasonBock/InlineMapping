using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace InlineMapping.Tests
{
	public static class MapReceiverTests
	{
		[Test]
		public static async Task FindCandidatesWhenAttributeIsDeclaredAsMapFromAsync()
		{
			var classDeclaration = (await SyntaxFactory.ParseSyntaxTree("[MapFrom] public class Source { }")
				.GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

			var receiver = new MapReceiver();
			receiver.OnVisitSyntaxNode(classDeclaration);

			Assert.Multiple(() =>
			{
				Assert.That(receiver.MapToCandidates.Count, Is.GreaterThan(0));
				Assert.That(receiver.MapToCandidates[0].Identifier.Text, Is.EqualTo("Source"));
			});
		}

		[Test]
		public static async Task FindCandidatesWhenAttributeIsDeclaredAsMapFromAttributeAsync()
		{
			var classDeclaration = (await SyntaxFactory.ParseSyntaxTree("[MapFromAttribute] public class Source { }")
				.GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

			var receiver = new MapReceiver();
			receiver.OnVisitSyntaxNode(classDeclaration);

			Assert.Multiple(() =>
			{
				Assert.That(receiver.MapToCandidates.Count, Is.GreaterThan(0));
				Assert.That(receiver.MapToCandidates[0].Identifier.Text, Is.EqualTo("Source"));
			});
		}

		[Test]
		public static async Task FindCandidatesWhenAttributeIsDeclaredAsMapToAsync()
		{
			var classDeclaration = (await SyntaxFactory.ParseSyntaxTree("[MapTo] public class Source { }")
				.GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

			var receiver = new MapReceiver();
			receiver.OnVisitSyntaxNode(classDeclaration);

			Assert.Multiple(() =>
			{
				Assert.That(receiver.MapToCandidates.Count, Is.GreaterThan(0));
				Assert.That(receiver.MapToCandidates[0].Identifier.Text, Is.EqualTo("Source"));
			});
		}

		[Test]
		public static async Task FindCandidatesWhenAttributeIsDeclaredAsMapToAttributeAsync()
		{
			var classDeclaration = (await SyntaxFactory.ParseSyntaxTree("[MapToAttribute] public class Source { }")
				.GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

			var receiver = new MapReceiver();
			receiver.OnVisitSyntaxNode(classDeclaration);

			Assert.Multiple(() =>
			{
				Assert.That(receiver.MapToCandidates.Count, Is.GreaterThan(0));
				Assert.That(receiver.MapToCandidates[0].Identifier.Text, Is.EqualTo("Source"));
			});
		}

		[Test]
		public static async Task FindCandidatesWhenAttributeIsDeclaredAsDummyAsync()
		{
			var classDeclaration = (await SyntaxFactory.ParseSyntaxTree("[Dummy] public class Source { }")
				.GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

			var receiver = new MapReceiver();
			receiver.OnVisitSyntaxNode(classDeclaration);

			Assert.Multiple(() =>
			{
				Assert.That(receiver.MapToCandidates.Count, Is.EqualTo(0));
			});
		}

		[Test]
		public static async Task FindCandidatesWithNoAttributes()
		{
			var classDeclaration = (await SyntaxFactory.ParseSyntaxTree("public class Source { }")
				.GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

			var receiver = new MapReceiver();
			receiver.OnVisitSyntaxNode(classDeclaration);

			Assert.Multiple(() =>
			{
				Assert.That(receiver.MapToCandidates.Count, Is.EqualTo(0));
			});
		}
	}
}