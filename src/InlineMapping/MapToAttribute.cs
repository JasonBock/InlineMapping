using System;

namespace InlineMapping
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
	public sealed class MapToAttribute
		: Attribute
	{
		public MapToAttribute(Type destination,
			ContainingNamespaceKind containingNamespaceKind = ContainingNamespaceKind.Source,
			MatchingPropertyTypeKind matchingPropertyTypeKind = MatchingPropertyTypeKind.Implicit) =>
			(this.Destination, this.ContainingNamespaceKind, this.MatchingPropertyTypeKind) =
				(destination, containingNamespaceKind, matchingPropertyTypeKind);

		public ContainingNamespaceKind ContainingNamespaceKind { get; }
		public Type Destination { get; }
		public MatchingPropertyTypeKind MatchingPropertyTypeKind { get; }
	}
}