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
			});
		}
	}
}