using InlineMapping;
using System;

Source source = new() { Id = Guid.NewGuid(), Name = "Joe", When = DateTime.Now };
var destination = source.MapToDestination();

Console.Out.WriteLine(destination.Id);
Console.Out.WriteLine(destination.Name);
Console.Out.WriteLine(destination.When);

[MapTo(typeof(Destination))]
public class Source
{
	public Guid Id { get; set; }
	public string? Name { get; set; }
	public DateTime When { get; set; }
}

public class Destination
{
	public Guid Id { get; set; }
	public string? Name { get; set; }
	public DateTime When { get; set; }
}