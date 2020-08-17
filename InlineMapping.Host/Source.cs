using InlineMapping.Metadata;
using System;

namespace InlineMapping.Host
{
	[MapTo(typeof(Destination))]
	public record Source
	{
		public Guid Id { get; init; }
		public string? Name { get; init; }
		public DateTime When { get; init; }
	}
}