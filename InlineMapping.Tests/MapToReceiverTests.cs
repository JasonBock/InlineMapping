using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace InlineMapping.Tests
{
	public static class MapToReceiverTests
	{
		[Test]
		public static async Task FindCandidatesWhenAttributeIsDeclaredAsMapToAsync()
		{
			var classDeclaration = (await SyntaxFactory.ParseSyntaxTree(
@"[MapTo]
public class Source { }").GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

			var receiver = new MapToReceiver();
			receiver.OnVisitSyntaxNode(classDeclaration);

			Assert.Multiple(() =>
			{
				Assert.That(receiver.Candidates.Count, Is.GreaterThan(0));
				Assert.That(receiver.Candidates[0].Identifier.Text, Is.EqualTo("Source"));
			});
		}

		[Test]
		public static async Task FindCandidatesWhenAttributeIsDeclaredAsMapToAttributeAsync()
		{
			var classDeclaration = (await SyntaxFactory.ParseSyntaxTree(
@"[MapToAttribute]
public class Source { }").GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

			var receiver = new MapToReceiver();
			receiver.OnVisitSyntaxNode(classDeclaration);

			Assert.Multiple(() =>
			{
				Assert.That(receiver.Candidates.Count, Is.GreaterThan(0));
				Assert.That(receiver.Candidates[0].Identifier.Text, Is.EqualTo("Source"));
			});
		}

		[Test]
		public static async Task FindCandidatesWhenAttributeIsDeclaredAsDummyAsync()
		{
			var classDeclaration = (await SyntaxFactory.ParseSyntaxTree(
@"[Dummy]
public class Source { }").GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

			var receiver = new MapToReceiver();
			receiver.OnVisitSyntaxNode(classDeclaration);

			Assert.Multiple(() =>
			{
				Assert.That(receiver.Candidates.Count, Is.EqualTo(0));
			});
		}

		[Test]
		public static async Task FindCandidatesWithNoAttributes()
		{
			var classDeclaration = (await SyntaxFactory.ParseSyntaxTree(
"public class Source { }").GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

			var receiver = new MapToReceiver();
			receiver.OnVisitSyntaxNode(classDeclaration);

			Assert.Multiple(() =>
			{
				Assert.That(receiver.Candidates.Count, Is.EqualTo(0));
			});
		}
	}
}