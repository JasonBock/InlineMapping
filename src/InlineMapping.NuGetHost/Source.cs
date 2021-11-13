namespace InlineMapping.NuGetHost;

[MapTo(typeof(Destination))]
public class Source
{
	public Guid Id { get; set; }
	public string? Name { get; set; }
	public DateTime When { get; set; }
}
