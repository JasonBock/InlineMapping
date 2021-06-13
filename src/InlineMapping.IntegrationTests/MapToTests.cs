using NUnit.Framework;
using System;

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

		[Test]
		public static void MapWithCustomConstuctor()
		{
			var source = new SourceForMapTo { Id = 3 };
			var destination = source.MapToDestinationForMapToWithCustomConstructor("a", Guid.NewGuid());

			Assert.That(destination.Id, Is.EqualTo(source.Id));
		}
	}

	[MapTo(typeof(DestinationForMapTo))]
	[MapTo(typeof(DestinationForMapToWithCustomConstructor))]
	public class SourceForMapTo
	{
		public int Id { get; set; }
	}

	public class DestinationForMapTo
	{
		public int Id { get; set; }
	}

	public class DestinationForMapToWithCustomConstructor
	{
		public DestinationForMapToWithCustomConstructor(string a, Guid b) { }

		public int Id { get; set; }
	}
}