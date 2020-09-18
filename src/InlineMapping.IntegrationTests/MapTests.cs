using NUnit.Framework;

namespace InlineMapping.IntegrationTests
{
	public static class MapTests
	{
		[Test]
		public static void Map()
		{
			var source = new Source { Id = 3 };
			var destination = new Destination(); // source.MapToDestination();

			Assert.Multiple(() =>
			{
				Assert.That(destination.Id, Is.EqualTo(3));
			});
		}
	}

	[MapTo(typeof(Destination))]
	public class Source
	{
		public int Id { get; set; }
	}

	public class Destination
	{
		public int Id { get; set; }
	}
}