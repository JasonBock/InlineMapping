using System;

namespace InlineMapping.Host
{
	public static class Program
	{
		public static void Main() 
		{
			var source = new Source
			{
				Id = Guid.NewGuid(),
				Name = "Joe",
				When = DateTime.Now,
			};

			var destination = source.MapToDestination();

			Console.Out.WriteLine($"Id: {source.Id} - {destination.Id}");
			Console.Out.WriteLine($"Name: {source.Name} - {destination.Name}");
			Console.Out.WriteLine($"source.When: {source.When}");
			Console.Out.WriteLine($"destination.Age: {destination.Age}");
		}
	}
}