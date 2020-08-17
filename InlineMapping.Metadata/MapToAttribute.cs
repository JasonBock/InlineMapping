using System;

namespace InlineMapping.Metadata
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class MapToAttribute
		: Attribute
	{
		public MapToAttribute(Type destination) =>
			this.Destination = destination;

		public Type Destination { get; }
	}
}