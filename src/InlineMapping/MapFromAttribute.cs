using System;

namespace InlineMapping
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public sealed class MapFromAttribute
		: Attribute
	{
		public MapFromAttribute(Type source) =>
			this.Source = source;

		public Type Source { get; }
	}
}