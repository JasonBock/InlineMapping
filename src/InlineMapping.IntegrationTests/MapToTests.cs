using NUnit.Framework;

namespace InlineMapping.IntegrationTests;

public static class MapToTests
{
	[Test]
	public static void MapWithClass()
	{
		var source = new SourceClassForMapTo { Id = 3 };
		var destination = source.MapToDestinationForMapTo();

		Assert.That(destination.Id, Is.EqualTo(source.Id));
	}

	[Test]
	public static void MapWithStruct()
	{
		var source = new SourceStructForMapTo { Id = 3 };
		var destination = source.MapToDestinationForMapTo();

		Assert.That(destination.Id, Is.EqualTo(source.Id));
	}
}

[MapTo(typeof(DestinationForMapTo))]
public sealed class SourceClassForMapTo
{
	public int Id { get; set; }
}

[MapTo(typeof(DestinationForMapTo))]
public struct SourceStructForMapTo
	: IEquatable<SourceStructForMapTo>
{
	public override bool Equals(object? obj) =>
		obj is SourceStructForMapTo other && this.Equals(other);

	public bool Equals(SourceStructForMapTo other) =>
		this.Id == other.Id;

	public override int GetHashCode() => this.Id.GetHashCode();

	public static bool operator ==(SourceStructForMapTo left, SourceStructForMapTo right) =>
		left.Equals(right);

	public static bool operator !=(SourceStructForMapTo left, SourceStructForMapTo right) =>
		!(left == right);

	public int Id { get; set; }
}

public class DestinationForMapTo
{
	public int Id { get; set; }
}