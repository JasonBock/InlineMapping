using InlineMapping.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

namespace InlineMapping
{
	internal sealed class MappingBuilder
	{
		public MappingBuilder(ITypeSymbol source, ITypeSymbol destination, ImmutableArray<string> maps, ConfigurationValues configurationValues) => 
			this.Text = MappingBuilder.Build(source, destination, maps, configurationValues);

		private static SourceText Build(ITypeSymbol source, ITypeSymbol destination, ImmutableArray<string> maps, ConfigurationValues configurationValues)
		{
			using var writer = new StringWriter();
			using var indentWriter = new IndentedTextWriter(writer, 
				configurationValues.IndentStyle == IndentStyle.Tab ? "\t" : new string(' ', (int)configurationValues.IndentSize));

			var usingStatements = new SortedSet<string>();

			if (!source.IsValueType)
			{
				usingStatements.Add("using System;");
			};

			if (!destination.ContainingNamespace.IsGlobalNamespace &&
				!source.ContainingNamespace.ToDisplayString().StartsWith(
					destination.ContainingNamespace.ToDisplayString(), StringComparison.InvariantCulture))
			{
				usingStatements.Add($"using {destination.ContainingNamespace.ToDisplayString()};");
			}

			foreach (var usingStatement in usingStatements)
			{
				indentWriter.WriteLine(usingStatement);
			}

			if (usingStatements.Count > 0)
			{
				indentWriter.WriteLine();
			}

			if (!source.ContainingNamespace.IsGlobalNamespace)
			{
				indentWriter.WriteLine($"namespace {source.ContainingNamespace.ToDisplayString()}");
				indentWriter.WriteLine("{");
				indentWriter.Indent++;
			}

			indentWriter.WriteLine($"public static partial class {source.Name}MapToExtensions");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"public static {destination.Name} MapTo{destination.Name}(this {source.Name} self) =>");
			indentWriter.Indent++;

			if (!source.IsValueType)
			{
				indentWriter.WriteLine("self is null ? throw new ArgumentNullException(nameof(self)) :");
				indentWriter.Indent++;
			}

			indentWriter.WriteLine($"new {destination.Name}");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			foreach (var map in maps)
			{
				indentWriter.WriteLine(map);
			}

			indentWriter.Indent--;
			indentWriter.WriteLine("};");

			if (!source.IsValueType)
			{
				indentWriter.Indent--;
			}

			indentWriter.Indent--;
			indentWriter.Indent--;
			indentWriter.WriteLine("}");

			if (!source.ContainingNamespace.IsGlobalNamespace)
			{
				indentWriter.Indent--;
				indentWriter.WriteLine("}");
			}

			return SourceText.From(writer.ToString(), Encoding.UTF8);
		}

		public SourceText Text { get; private set; }
	}
}