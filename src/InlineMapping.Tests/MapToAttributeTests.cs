using NUnit.Framework;

namespace InlineMapping.Tests
{
	public static class MapToAttributeTests
	{
		[Test]
		public static void Create()
		{
			var attribute = new MapToAttribute(typeof(MapToAttributeTests));
			Assert.Multiple(() =>
			{
				Assert.That(attribute.Destination, Is.EqualTo(typeof(MapToAttributeTests)));
				Assert.That(attribute.Kind, Is.EqualTo(ContainingNamespaceKind.Source));
			});
		}

		[Test]
		public static void CreateWithGlobalKind()
		{
			var attribute = new MapToAttribute(typeof(MapToAttributeTests), ContainingNamespaceKind.Global);
			Assert.Multiple(() =>
			{
				Assert.That(attribute.Destination, Is.EqualTo(typeof(MapToAttributeTests)));
				Assert.That(attribute.Kind, Is.EqualTo(ContainingNamespaceKind.Global));
			});
		}
	}
}