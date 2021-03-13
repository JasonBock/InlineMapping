namespace InlineMapping
{
	internal static class HelpUrlBuilder
	{
		internal static string Build(string identifier, string title) =>
		  $"https://github.com/JasonBock/InlineMapping/tree/main/docs/{identifier}-{title}.md";
	}
}