using InlineMapping.NuGetHost;

var source = new Source { Id = Guid.NewGuid(), Name = "Joe", When = DateTime.Now };
var destination = source.MapToDestination();

Console.Out.WriteLine(destination.Id);
Console.Out.WriteLine(destination.Name);
Console.Out.WriteLine(destination.When);