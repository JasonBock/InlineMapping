using System;

namespace InlineMapping
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class MapAttribute
		: Attribute
	{
		public MapAttribute(Type source, Type destination) =>
			(this.Source, this.Destination) = (source, destination);

		public Type Destination { get; }
		public Type Source { get; }
	}
}