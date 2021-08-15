using NUnit.Framework;

namespace InlineMapping.Tests
{
	public static class MappingContextTests
	{
		[Test]
		public static void Create()
		{
			var containingNamespaceKind = ContainingNamespaceKind.Global;
			var matchingPropertyTypeKind = MatchingPropertyTypeKind.Exact;

			var context = new MappingContext(containingNamespaceKind, matchingPropertyTypeKind);

			Assert.Multiple(() =>
			{
				Assert.That(context.ContainingNamespaceKind, Is.EqualTo(containingNamespaceKind), nameof(MappingContext.ContainingNamespaceKind));
				Assert.That(context.MatchingPropertyTypeKind, Is.EqualTo(matchingPropertyTypeKind), nameof(MappingContext.MatchingPropertyTypeKind));
			});
		}
	}
}