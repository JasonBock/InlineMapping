using NUnit.Framework;
using System;

namespace InlineMapping.IntegrationTests
{
	public static class MultipleConstructorTests
	{
		[Test]
		public static void MapWithNoConstructorArguments()
		{
			var source = new SourceForMultipleConstructors { Id = 3 };
			var destination = source.MapToDestinationWithMultipleConstructors();

			Assert.Multiple(() =>
			{
				Assert.That(destination.ConstructorA, Is.Null);
				Assert.That(destination.ConstructorB, Is.EqualTo(Guid.Empty));
				Assert.That(destination.Id, Is.EqualTo(source.Id));
			});
		}
		
		[Test]
		public static void MapWithOneConstructorArgument()
		{
			var source = new SourceForMultipleConstructors { Id = 3 };
			var destination = source.MapToDestinationWithMultipleConstructors("a");

			Assert.Multiple(() =>
			{
				Assert.That(destination.ConstructorA, Is.EqualTo("a"));
				Assert.That(destination.ConstructorB, Is.EqualTo(Guid.Empty));
				Assert.That(destination.Id, Is.EqualTo(source.Id));
			});
		}

		[Test]
		public static void MapWithTwoConstructorArguments()
		{
			var source = new SourceForMultipleConstructors { Id = 3 };
			var b = Guid.NewGuid();
			var destination = source.MapToDestinationWithMultipleConstructors("a", b);

			Assert.Multiple(() =>
			{
				Assert.That(destination.ConstructorA, Is.EqualTo("a"));
				Assert.That(destination.ConstructorB, Is.EqualTo(b));
				Assert.That(destination.Id, Is.EqualTo(source.Id));
			});
		}
	}

	[MapTo(typeof(DestinationWithMultipleConstructors))]
	public sealed class SourceForMultipleConstructors
	{
		public int Id { get; set; }
	}

	public sealed class DestinationWithMultipleConstructors
	{
		public DestinationWithMultipleConstructors() { }

		public DestinationWithMultipleConstructors(string a) =>
			this.ConstructorA = a;

		public DestinationWithMultipleConstructors(string a, Guid b) =>
			(this.ConstructorA, this.ConstructorB) =
				(a, b);

		public string? ConstructorA { get; }
		public Guid ConstructorB { get; }
		public int Id { get; set; }
	}
}