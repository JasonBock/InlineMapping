namespace InlineMapping
{
	public sealed class MappingContext
	{
		public MappingContext(ContainingNamespaceKind containingNamespaceKind,
			MatchingPropertyTypeKind matchingPropertyTypeKind) =>
			(this.ContainingNamespaceKind, this.MatchingPropertyTypeKind) =
				(containingNamespaceKind, matchingPropertyTypeKind);
		public ContainingNamespaceKind ContainingNamespaceKind { get; }
		public MatchingPropertyTypeKind MatchingPropertyTypeKind { get; }
	}
}