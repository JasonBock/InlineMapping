using InlineMapping.Host;
using System;

var source = new Source
{
	Id = Guid.NewGuid(),
	Name = "Jeff",
	When = DateTime.Now,
};

var destination = source.MapToDestination();

Console.Out.WriteLine($"Id: {source.Id} - {destination.Id}");
Console.Out.WriteLine($"Name: {source.Name} - {destination.Name}");
Console.Out.WriteLine($"source.When: {source.When}");
Console.Out.WriteLine($"destination.Age: {destination.Age}");