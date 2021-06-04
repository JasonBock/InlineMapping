using NUnit.Framework;

namespace InlineMapping.Tests
{
	public static class MapFromAttributeTests
	{
		[Test]
		public static void Create()
		{
			var attribute = new MapFromAttribute(typeof(MapFromAttributeTests));
			Assert.Multiple(() =>
			{
				Assert.That(attribute.Source, Is.EqualTo(typeof(MapFromAttributeTests)));
				Assert.That(attribute.Kind, Is.EqualTo(ContainingNamespaceKind.Source));
			});
		}

		[Test]
		public static void CreateWithGlobalKind()
		{
			var attribute = new MapFromAttribute(typeof(MapFromAttributeTests), ContainingNamespaceKind.Global);
			Assert.Multiple(() =>
			{
				Assert.That(attribute.Source, Is.EqualTo(typeof(MapFromAttributeTests)));
				Assert.That(attribute.Kind, Is.EqualTo(ContainingNamespaceKind.Global));
			});
		}
	}
}