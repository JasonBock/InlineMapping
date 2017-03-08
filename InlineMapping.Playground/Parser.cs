using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Threading.Tasks;

namespace InlineMapping.Playground
{
	internal static class Parser
	{
		internal static async Task<Tuple<SyntaxNode, SemanticModel>> GetRootAndModelAsync(string code)
		{
			var tree = CSharpSyntaxTree.ParseText(code);

			var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString("N"),
				syntaxTrees: new[] { tree },
				references: new[]
				{
					MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
					MetadataReference.CreateFromFile(typeof(InlineMapper).Assembly.Location)
				});

			var model = compilation.GetSemanticModel(tree);
			var root = await tree.GetRootAsync().ConfigureAwait(false);

			return new Tuple<SyntaxNode, SemanticModel>(root, model);
		}
	}
}
