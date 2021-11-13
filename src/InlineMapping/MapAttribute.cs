namespace InlineMapping;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class MapAttribute
	 : Attribute
{
   public MapAttribute(Type source, Type destination,
	   ContainingNamespaceKind containingNamespaceKind = ContainingNamespaceKind.Source,
	   MatchingPropertyTypeKind matchingPropertyTypeKind = MatchingPropertyTypeKind.Implicit) =>
	   (this.Source, this.Destination, this.ContainingNamespaceKind, this.MatchingPropertyTypeKind) =
		   (source, destination, containingNamespaceKind, matchingPropertyTypeKind);

   public Type Destination { get; }
   public ContainingNamespaceKind ContainingNamespaceKind { get; }
   public MatchingPropertyTypeKind MatchingPropertyTypeKind { get; }
   public Type Source { get; }
}