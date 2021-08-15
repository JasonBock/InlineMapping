using System;

namespace InlineMapping
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public sealed class MapFromAttribute
		: Attribute
	{
		public MapFromAttribute(Type source,
			ContainingNamespaceKind containingNamespaceKind = ContainingNamespaceKind.Source,
			MatchingPropertyTypeKind matchingPropertyTypeKind = MatchingPropertyTypeKind.SubType) =>
			(this.Source, this.ContainingNamespaceKind, this.MatchingPropertyTypeKind) = 
				(source, containingNamespaceKind, matchingPropertyTypeKind);

		public ContainingNamespaceKind ContainingNamespaceKind { get; }
		public MatchingPropertyTypeKind MatchingPropertyTypeKind { get; }
		public Type Source { get; }
	}
}