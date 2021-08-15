using NUnit.Framework;

namespace InlineMapping.Tests
{
	public static class MapAttributeTests
	{
		[Test]
		public static void Create()
		{
			var attribute = new MapAttribute(typeof(MapFromAttributeTests), typeof(MapToAttributeTests));

			Assert.Multiple(() =>
			{
				Assert.That(attribute.Source, Is.EqualTo(typeof(MapFromAttributeTests)));
				Assert.That(attribute.Destination, Is.EqualTo(typeof(MapToAttributeTests)));
				Assert.That(attribute.ContainingNamespaceKind, Is.EqualTo(ContainingNamespaceKind.Source));
				Assert.That(attribute.MatchingPropertyTypeKind, Is.EqualTo(MatchingPropertyTypeKind.SubType));
			});
		}

		[Test]
		public static void CreateWithGlobalKind()
		{
			var attribute = new MapAttribute(typeof(MapFromAttributeTests), typeof(MapToAttributeTests), 
				containingNamespaceKind: ContainingNamespaceKind.Global);

			Assert.Multiple(() =>
			{
				Assert.That(attribute.Source, Is.EqualTo(typeof(MapFromAttributeTests)));
				Assert.That(attribute.Destination, Is.EqualTo(typeof(MapToAttributeTests)));
				Assert.That(attribute.ContainingNamespaceKind, Is.EqualTo(ContainingNamespaceKind.Global));
				Assert.That(attribute.MatchingPropertyTypeKind, Is.EqualTo(MatchingPropertyTypeKind.SubType));
			});
		}

		[Test]
		public static void CreateWithExactMatchingType()
		{
			var attribute = new MapAttribute(typeof(MapFromAttributeTests), typeof(MapToAttributeTests), 
				matchingPropertyTypeKind: MatchingPropertyTypeKind.Exact);

			Assert.Multiple(() =>
			{
				Assert.That(attribute.Source, Is.EqualTo(typeof(MapFromAttributeTests)));
				Assert.That(attribute.Destination, Is.EqualTo(typeof(MapToAttributeTests)));
				Assert.That(attribute.ContainingNamespaceKind, Is.EqualTo(ContainingNamespaceKind.Source));
				Assert.That(attribute.MatchingPropertyTypeKind, Is.EqualTo(MatchingPropertyTypeKind.Exact));
			});
		}
	}
}