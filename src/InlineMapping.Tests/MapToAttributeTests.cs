using NUnit.Framework;

namespace InlineMapping.Tests
{
	public static class MapToAttributeTests
	{
		[Test]
		public static void Create()
		{
			var attribute = new MapToAttribute(typeof(MapToAttributeTests));
			Assert.That(attribute.Destination, Is.EqualTo(typeof(MapToAttributeTests)));
		}
	}
}