using System;

namespace InlineMapping.Host
{
	public record Destination
	{
		public uint Age { get; init; }
		public Guid Id { get; init; }
		public string? Name { get; init; }
	}
}