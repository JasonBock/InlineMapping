using System;

namespace InlineMapping
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