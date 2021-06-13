using InlineMapping;
using InlineMapping.IntegrationTests;
using NUnit.Framework;
using System;

[assembly: Map(typeof(SourceForMap), typeof(DestinationForMap))]
[assembly: Map(typeof(SourceForMap), typeof(DestinationForMapWithCustomConstructor))]

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

		[Test]
		public static void MapWithCustomConstructor()
		{
			var source = new SourceForMap { Id = 3 };
			var destination = source.MapToDestinationForMapWithCustomConstructor("a", Guid.NewGuid());

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

	public class DestinationForMapWithCustomConstructor
	{
		public DestinationForMapWithCustomConstructor(string a, Guid b) { }

		public int Id { get; set; }
	}
}