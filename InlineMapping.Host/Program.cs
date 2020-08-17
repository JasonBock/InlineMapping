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

			//var destination = source.MapToDestination();
		}
	}
}