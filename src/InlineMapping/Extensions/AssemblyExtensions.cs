using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using System.Reflection;
using System.Text;

namespace InlineMapping.Extensions
{
	internal static class AssemblyExtensions
	{
		internal static (ITypeSymbol?, Compilation) LoadSymbol(this Assembly self, string typeResourceName, string typeName,
			GeneratorExecutionContext context)
		{
			static SourceText GetText(Assembly assembly, string typeResourceName)
			{
				using var stream = assembly.GetManifestResourceStream(typeResourceName);
				using var reader = new StreamReader(stream);
				return SourceText.From(reader.ReadToEnd(), Encoding.UTF8);
			}

			var code = GetText(self, typeResourceName);
			context.AddSource(typeResourceName, code);

			var options = (context.Compilation as CSharpCompilation)!.SyntaxTrees[0].Options as CSharpParseOptions;
			var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code, options));

			return (compilation.GetTypeByMetadataName(typeName), compilation);
		}
	}
}