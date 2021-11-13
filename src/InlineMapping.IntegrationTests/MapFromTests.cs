using NUnit.Framework;

namespace InlineMapping.IntegrationTests;

public static class MapFromTests
{
   [Test]
   public static void MapWithClass()
   {
	  var source = new SourceClassForMapFrom { Id = 3 };
	  var destination = source.MapToDestinationForMapFrom();

	  Assert.That(destination.Id, Is.EqualTo(source.Id));
   }

   [Test]
   public static void MapWithStruct()
   {
	  var source = new SourceStructForMapFrom { Id = 3 };
	  var destination = source.MapToDestinationForMapFrom();

	  Assert.That(destination.Id, Is.EqualTo(source.Id));
   }
}

public sealed class SourceClassForMapFrom
{
   public int Id { get; set; }
}

public struct SourceStructForMapFrom
{
   public int Id { get; set; }
}

[MapFrom(typeof(SourceClassForMapFrom))]
[MapFrom(typeof(SourceStructForMapFrom))]
public class DestinationForMapFrom
{
   public int Id { get; set; }
}