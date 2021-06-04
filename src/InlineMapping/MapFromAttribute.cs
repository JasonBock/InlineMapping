using System;

namespace InlineMapping
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public sealed class MapFromAttribute
		: Attribute
	{
		public MapFromAttribute(Type source, ContainingNamespaceKind kind = ContainingNamespaceKind.Source) =>
			(this.Source, this.Kind) = (source, kind);

		public ContainingNamespaceKind Kind { get; }
		public Type Source { get; }
	}
}