namespace InlineMapping
{
	internal static class HelpUrlBuilder
	{
		internal static string Build(string identifier, string title) =>
		  $"https://github.com/JasonBock/InlineMapping/tree/master/docs/{identifier}-{title}.md";
	}
}