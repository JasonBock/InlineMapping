using System;

namespace InlineMapping
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public sealed class MapToAttribute
		: Attribute
	{
		public MapToAttribute(Type destination, ContainingNamespaceKind kind = ContainingNamespaceKind.Source) =>
			(this.Destination, this.Kind) = (destination, kind);

		public Type Destination { get; }
		public ContainingNamespaceKind Kind { get; }
	}
}