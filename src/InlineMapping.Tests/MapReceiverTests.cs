using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace InlineMapping.Tests
{
	public static class MapReceiverTests
	{
		[TestCase("[MapFrom] public class Source { }", 1, 0)]
		[TestCase("[MapFromAttribute] public class Source { }", 1, 0)]
		[TestCase("[MapTo] public class Source { }", 0, 1)]
		[TestCase("[MapToAttribute] public class Source { }", 0, 1)]
		public static async Task FindCandidatesWhenAttributeIsTargetingATypeAsync(
			string code, int expectedFromCount, int expectedToCount)
		{
			var classDeclaration = (await SyntaxFactory.ParseSyntaxTree(code)
				.GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

			var receiver = new MapReceiver();
			receiver.OnVisitSyntaxNode(classDeclaration);

			Assert.Multiple(() =>
			{
				Assert.That(receiver.MapFromCandidates.Count, Is.EqualTo(expectedFromCount));
				Assert.That(receiver.MapToCandidates.Count, Is.EqualTo(expectedToCount));
				Assert.That(receiver.MapCandidates.Count, Is.EqualTo(0));
			});
		}

		[TestCase("[assembly: Map]")]
		[TestCase("[assembly: MapAttribute]")]
		public static async Task FindCandidatesWhenAttributeIsTargetingAnAssemblyAsync(string code)
		{
			var attributeDeclaration = (await SyntaxFactory.ParseSyntaxTree(code)
				.GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<AttributeSyntax>().First();

			var receiver = new MapReceiver();
			receiver.OnVisitSyntaxNode(attributeDeclaration);

			Assert.Multiple(() =>
			{
				Assert.That(receiver.MapFromCandidates.Count, Is.EqualTo(0));
				Assert.That(receiver.MapToCandidates.Count, Is.EqualTo(0));
				Assert.That(receiver.MapCandidates.Count, Is.EqualTo(1));
			});
		}

		[TestCase("[Dummy] public class Source { }")]
		[TestCase("public class Source { }")]
		public static async Task FindCandidatesWithNoMatchesAsync(string code)
		{
			var classDeclaration = (await SyntaxFactory.ParseSyntaxTree(code)
				.GetRootAsync().ConfigureAwait(false)).DescendantNodes(_ => true).OfType<ClassDeclarationSyntax>().First();

			var receiver = new MapReceiver();
			receiver.OnVisitSyntaxNode(classDeclaration);

			Assert.Multiple(() =>
			{
				Assert.That(receiver.MapFromCandidates.Count, Is.EqualTo(0));
				Assert.That(receiver.MapToCandidates.Count, Is.EqualTo(0));
				Assert.That(receiver.MapCandidates.Count, Is.EqualTo(0));
			});
		}
	}
}