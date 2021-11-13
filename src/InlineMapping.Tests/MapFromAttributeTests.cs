using NUnit.Framework;

namespace InlineMapping.Tests;

public static class MapFromAttributeTests
{
   [Test]
   public static void Create()
   {
	  var attribute = new MapFromAttribute(typeof(MapFromAttributeTests));
	  Assert.Multiple(() =>
	  {
		 Assert.That(attribute.Source, Is.EqualTo(typeof(MapFromAttributeTests)));
		 Assert.That(attribute.ContainingNamespaceKind, Is.EqualTo(ContainingNamespaceKind.Source));
		 Assert.That(attribute.MatchingPropertyTypeKind, Is.EqualTo(MatchingPropertyTypeKind.Implicit));
	  });
   }

   [Test]
   public static void CreateWithGlobalKind()
   {
	  var attribute = new MapFromAttribute(typeof(MapFromAttributeTests),
		  containingNamespaceKind: ContainingNamespaceKind.Global);
	  Assert.Multiple(() =>
	  {
		 Assert.That(attribute.Source, Is.EqualTo(typeof(MapFromAttributeTests)));
		 Assert.That(attribute.ContainingNamespaceKind, Is.EqualTo(ContainingNamespaceKind.Global));
		 Assert.That(attribute.MatchingPropertyTypeKind, Is.EqualTo(MatchingPropertyTypeKind.Implicit));
	  });
   }

   [Test]
   public static void CreateWithExactMatchingType()
   {
	  var attribute = new MapFromAttribute(typeof(MapFromAttributeTests),
		  matchingPropertyTypeKind: MatchingPropertyTypeKind.Exact);
	  Assert.Multiple(() =>
	  {
		 Assert.That(attribute.Source, Is.EqualTo(typeof(MapFromAttributeTests)));
		 Assert.That(attribute.ContainingNamespaceKind, Is.EqualTo(ContainingNamespaceKind.Source));
		 Assert.That(attribute.MatchingPropertyTypeKind, Is.EqualTo(MatchingPropertyTypeKind.Exact));
	  });
   }
}