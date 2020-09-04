using System;

namespace InlineMapping.PerformanceTests
{
	[MapTo(typeof(Destination))]
	public class Source
	{
		public uint Age { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
		public byte[]? Buffer { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
		public Guid Id { get; set; }
		public string? Name { get; set; }
		public DateTime When { get; set; }
	}
}