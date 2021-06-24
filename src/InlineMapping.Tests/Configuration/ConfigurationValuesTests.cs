using InlineMapping.Configuration;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using Rocks;
using System.Diagnostics.CodeAnalysis;

namespace InlineMapping.Tests.Configuration
{
	public static class ConfigurationValuesTests
	{
		[TestCase(nameof(IndentStyle.Space), true, "4", true, 4u, IndentStyle.Space)]
		[TestCase("garbage", true, "garbage", true, 3u, IndentStyle.Tab)]
		[TestCase("garbage", false, "garbage", false, 3u, IndentStyle.Tab)]
		public static void Create(string indentStyleValue, bool indentStyleCallbackReturn,
			string indentSizeValue, bool indentSizeCallbackReturn,
			uint expectedIndentSize, IndentStyle expectedIndentStyle)
		{
			bool IndentStyleCallback(string key, [NotNullWhen(true)] out string? value)
			{
				value = indentStyleValue;
				return indentStyleCallbackReturn;
			}

			bool IndentSizeCallback(string key, [NotNullWhen(true)] out string? value)
			{
				value = indentSizeValue;
				return indentSizeCallbackReturn;
			}

			var tree = SyntaxFactory.ParseSyntaxTree("var id = 3;");

			var options = Rock.Create<AnalyzerConfigOptions>();
			options.Methods().TryGetValue("indent_size", Arg.Any<string?>()).Callback(IndentSizeCallback);
			options.Methods().TryGetValue("indent_style", Arg.Any<string?>()).Callback(IndentStyleCallback);

			var provider = Rock.Create<AnalyzerConfigOptionsProvider>();
			provider.Methods().GetOptions(tree).Returns(options.Instance());

			var configuration = new ConfigurationValues(provider.Instance(), tree);

			Assert.Multiple(() =>
			{
				Assert.That(configuration.IndentSize, Is.EqualTo(expectedIndentSize), nameof(configuration.IndentSize));
				Assert.That(configuration.IndentStyle, Is.EqualTo(expectedIndentStyle), nameof(configuration.IndentSize));
			});

			provider.Verify();
			options.Verify();
		}
	}
}