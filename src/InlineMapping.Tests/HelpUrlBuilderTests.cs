using NUnit.Framework;

namespace InlineMapping.Tests;

public static class HelpUrlBuilderTests
{
   [Test]
   public static void Create() =>
	   Assert.That(HelpUrlBuilder.Build("a", "b"),
		   Is.EqualTo("https://github.com/JasonBock/InlineMapping/tree/main/docs/a-b.md"));
}