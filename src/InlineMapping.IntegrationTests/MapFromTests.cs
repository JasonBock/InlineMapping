using NUnit.Framework;
using System;

namespace InlineMapping.IntegrationTests
{
	public static class MapFromTests
	{
		[Test]
		public static void Map()
		{
			var source = new SourceForMapFrom { Id = 3 };
			var destination = source.MapToDestinationForMapFrom();

			Assert.That(destination.Id, Is.EqualTo(source.Id));
		}

		[Test]
		public static void MapWithCustomConstructor()
		{
			var source = new SourceForMapFrom { Id = 3 };
			var destination = source.MapToDestinationForMapFromWithCustomConstructor("a", Guid.NewGuid());

			Assert.That(destination.Id, Is.EqualTo(source.Id));
		}
	}

	public class SourceForMapFrom
	{
		public int Id { get; set; }
	}

	[MapFrom(typeof(SourceForMapFrom))]
	public class DestinationForMapFrom
	{
		public int Id { get; set; }
	}

	[MapFrom(typeof(SourceForMapFrom))]
	public class DestinationForMapFromWithCustomConstructor
	{
		public DestinationForMapFromWithCustomConstructor(string a, Guid b) { }

		public int Id { get; set; }
	}
}