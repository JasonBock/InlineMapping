using InlineMapping;
using InlineMapping.IntegrationTests;
using NUnit.Framework;

[assembly: Map(typeof(SourceForMap), typeof(DestinationForMap))]

namespace InlineMapping.IntegrationTests
{
	public static class MapTests
	{
		[Test]
		public static void Map()
		{
			var source = new SourceForMap { Id = 3 };
			var destination = source.MapToDestinationForMap();

			Assert.That(destination.Id, Is.EqualTo(source.Id));
		}
	}

	public class SourceForMap
	{
		public int Id { get; set; }
	}

	public class DestinationForMap
	{
		public int Id { get; set; }
	}
}