using System;

namespace InlineMapping
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class MapAttribute
		: Attribute
	{
		public MapAttribute(Type source, Type destination, ContainingNamespaceKind kind = ContainingNamespaceKind.Source) =>
			(this.Source, this.Destination, this.Kind) = (source, destination, kind);

		public Type Destination { get; }
		public ContainingNamespaceKind Kind { get; }
		public Type Source { get; }
	}
}