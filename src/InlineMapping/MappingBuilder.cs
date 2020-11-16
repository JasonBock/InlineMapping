using Microsoft.CodeAnalysis.Text;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InlineMapping
{
	internal sealed class MappingBuilder
	{
		public MappingBuilder(MappingInformation information) => 
			this.Text = MappingBuilder.Build(information);

		private static SourceText Build(MappingInformation information)
		{
			using var writer = new StringWriter();
			// TODO: Can we read .editorconfig to figure out the space/tab + indention
			using var indentWriter = new IndentedTextWriter(writer, "	");

			var usingStatements = new SortedSet<string>();

			if (!information.SourceType.IsValueType)
			{
				usingStatements.Add("using System;");
			};

			if (!information.DestinationType.ContainingNamespace.IsGlobalNamespace &&
				!information.SourceType.ContainingNamespace.ToDisplayString().StartsWith(
					information.DestinationType.ContainingNamespace.ToDisplayString(), StringComparison.InvariantCulture))
			{
				usingStatements.Add($"using {information.DestinationType.ContainingNamespace.ToDisplayString()};");
			}

			foreach (var usingStatement in usingStatements)
			{
				indentWriter.WriteLine(usingStatement);
			}

			if (usingStatements.Count > 0)
			{
				indentWriter.WriteLine();
			}

			if (!information.SourceType.ContainingNamespace.IsGlobalNamespace)
			{
				indentWriter.WriteLine($"namespace {information.SourceType.ContainingNamespace.ToDisplayString()}");
				indentWriter.WriteLine("{");
				indentWriter.Indent++;
			}

			indentWriter.WriteLine($"public static partial class {information.SourceType.Name}MapToExtensions");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"public static {information.DestinationType.Name} MapTo{information.DestinationType.Name}(this {information.SourceType.Name} self) =>");
			indentWriter.Indent++;

			if (!information.SourceType.IsValueType)
			{
				indentWriter.WriteLine("self is null ? throw new ArgumentNullException(nameof(self)) :");
				indentWriter.Indent++;
			}

			indentWriter.WriteLine($"new {information.DestinationType.Name}");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			foreach (var map in information.Maps)
			{
				indentWriter.WriteLine(map);
			}

			indentWriter.Indent--;
			indentWriter.WriteLine("};");

			if (!information.SourceType.IsValueType)
			{
				indentWriter.Indent--;
			}

			indentWriter.Indent--;
			indentWriter.Indent--;
			indentWriter.WriteLine("}");

			if (!information.SourceType.ContainingNamespace.IsGlobalNamespace)
			{
				indentWriter.Indent--;
				indentWriter.WriteLine("}");
			}

			return SourceText.From(writer.ToString(), Encoding.UTF8);
		}

		public SourceText Text { get; private set; }
	}
}