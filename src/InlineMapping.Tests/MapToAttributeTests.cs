using NUnit.Framework;

namespace InlineMapping.Tests;

public static class MapToAttributeTests
{
   [Test]
   public static void Create()
   {
	  var attribute = new MapToAttribute(typeof(MapToAttributeTests));
	  Assert.Multiple(() =>
	  {
		 Assert.That(attribute.Destination, Is.EqualTo(typeof(MapToAttributeTests)));
		 Assert.That(attribute.ContainingNamespaceKind, Is.EqualTo(ContainingNamespaceKind.Source));
		 Assert.That(attribute.MatchingPropertyTypeKind, Is.EqualTo(MatchingPropertyTypeKind.Implicit));
	  });
   }

   [Test]
   public static void CreateWithGlobalKind()
   {
	  var attribute = new MapToAttribute(typeof(MapToAttributeTests),
		  containingNamespaceKind: ContainingNamespaceKind.Global);
	  Assert.Multiple(() =>
	  {
		 Assert.That(attribute.Destination, Is.EqualTo(typeof(MapToAttributeTests)));
		 Assert.That(attribute.ContainingNamespaceKind, Is.EqualTo(ContainingNamespaceKind.Global));
		 Assert.That(attribute.MatchingPropertyTypeKind, Is.EqualTo(MatchingPropertyTypeKind.Implicit));
	  });
   }

   [Test]
   public static void CreateWithExactMatchingType()
   {
	  var attribute = new MapToAttribute(typeof(MapToAttributeTests),
		  matchingPropertyTypeKind: MatchingPropertyTypeKind.Exact);
	  Assert.Multiple(() =>
	  {
		 Assert.That(attribute.Destination, Is.EqualTo(typeof(MapToAttributeTests)));
		 Assert.That(attribute.ContainingNamespaceKind, Is.EqualTo(ContainingNamespaceKind.Source));
		 Assert.That(attribute.MatchingPropertyTypeKind, Is.EqualTo(MatchingPropertyTypeKind.Exact));
	  });
   }
}