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
	: IEquatable<SourceStructForMapFrom>
{
	public override bool Equals(object? obj) =>
		obj is SourceStructForMapFrom other && this.Equals(other);

	public bool Equals(SourceStructForMapFrom other) =>
		this.Id == other.Id;

	public override int GetHashCode() => this.Id.GetHashCode();

	public static bool operator ==(SourceStructForMapFrom left, SourceStructForMapFrom right) =>
		left.Equals(right);

	public static bool operator !=(SourceStructForMapFrom left, SourceStructForMapFrom right) =>
		!(left == right);

	public int Id { get; set; }
}

[MapFrom(typeof(SourceClassForMapFrom))]
[MapFrom(typeof(SourceStructForMapFrom))]
public class DestinationForMapFrom
{
	public int Id { get; set; }
}