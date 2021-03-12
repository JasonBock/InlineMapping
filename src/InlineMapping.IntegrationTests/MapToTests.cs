using NUnit.Framework;

namespace InlineMapping.IntegrationTests
{
	public static class MapToTests
	{
		[Test]
		public static void Map()
		{
			var source = new SourceForMapTo { Id = 3 };
			var destination = source.MapToDestinationForMapTo();

			Assert.That(destination.Id, Is.EqualTo(source.Id));
		}
	}

	[MapTo(typeof(DestinationForMapTo))]
	public class SourceForMapTo
	{
		public int Id { get; set; }
	}

	public class DestinationForMapTo
	{
		public int Id { get; set; }
	}
}