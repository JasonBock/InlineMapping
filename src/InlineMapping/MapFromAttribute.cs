using System;

namespace InlineMapping
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class MapFromAttribute
		: Attribute
	{
		public MapFromAttribute(Type source) =>
			this.Source = source;

		public Type Source { get; }
	}
}