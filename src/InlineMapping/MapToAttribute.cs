using System;

namespace InlineMapping
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public sealed class MapToAttribute
		: Attribute
	{
		public MapToAttribute(Type destination) =>
			this.Destination = destination;

		public Type Destination { get; }
	}
}