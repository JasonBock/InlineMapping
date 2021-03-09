using NUnit.Framework;

namespace InlineMapping.Tests
{
	public static class MapFromAttributeTests
	{
		[Test]
		public static void Create()
		{
			var attribute = new MapFromAttribute(typeof(MapFromAttributeTests));
			Assert.That(attribute.Source, Is.EqualTo(typeof(MapFromAttributeTests)));
		}
	}
}