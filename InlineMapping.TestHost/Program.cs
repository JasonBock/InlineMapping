using InlineMapping.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace InlineMapping.TestHost
{
	public static class Program
	{
		public static void Main() => Program.GenerateHappyPath();

		public static void GenerateHappyPath()
		{
			// TODO: Do this with records and init
			var (diagnostics, output) = Program.GetGeneratedOutput(
@"using InlineMapping.Metadata;
using System;

namespace TestItOut
{
	public class Destination
	{
		public uint Age { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
		public byte[]? Buffer { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
		public Guid Id { get; set; }
		public string? Name { get; set; }
		public DateTime When { get; set; }
	}

	[MapTo(typeof(Destination))]
	public class Source
	{
		public uint Age { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
		public byte[]? Buffer { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
		public Guid Id { get; set; }
		public string? Name { get; set; }
		public DateTime When { get; set; }
	}
}");

			Console.Out.WriteLine($"diagnostics.Length is {diagnostics.Length}.");
			Console.Out.WriteLine(output);
		}

		private static (ImmutableArray<Diagnostic>, string) GetGeneratedOutput(string source)
		{
			var syntaxTree = CSharpSyntaxTree.ParseText(source);
			var references = AppDomain.CurrentDomain.GetAssemblies()
				.Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
				.Select(_ => MetadataReference.CreateFromFile(_.Location))
				.Concat(new[] { MetadataReference.CreateFromFile(typeof(MapToAttribute).Assembly.Location) });
			var compilation = CSharpCompilation.Create("generator", new SyntaxTree[] { syntaxTree },
				references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
			var generator = new MapToGenerator();

			var driver = new CSharpGeneratorDriver(compilation.SyntaxTrees[0].Options,
				ImmutableArray.Create<ISourceGenerator>(generator), default!, ImmutableArray<AdditionalText>.Empty);
			driver.RunFullGeneration(compilation, out var outputCompilation, out var diagnostics);

			return (diagnostics, outputCompilation.SyntaxTrees.Last().ToString());
		}
	}
}
