using InlineMapping;
using InlineMapping.IntegrationTests;
using NUnit.Framework;

[assembly: Map(typeof(SourceClassForMap), typeof(DestinationForMap))]
[assembly: Map(typeof(SourceStructForMap), typeof(DestinationForMap))]

namespace InlineMapping.IntegrationTests;

public static class MapTests
{
   [Test]
   public static void MapWithClass()
   {
	  var source = new SourceClassForMap { Id = 3 };
	  var destination = source.MapToDestinationForMap();

	  Assert.That(destination.Id, Is.EqualTo(source.Id));
   }

   [Test]
   public static void MapWithStruct()
   {
	  var source = new SourceStructForMap { Id = 3 };
	  var destination = source.MapToDestinationForMap();

	  Assert.That(destination.Id, Is.EqualTo(source.Id));
   }
}

public sealed class SourceClassForMap
{
   public int Id { get; set; }
}

public struct SourceStructForMap
{
   public int Id { get; set; }
}

public class DestinationForMap
{
   public int Id { get; set; }
}