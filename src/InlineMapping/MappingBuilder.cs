using InlineMapping.Configuration;
using InlineMapping.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace InlineMapping
{
	internal sealed class MappingBuilder
	{
		public MappingBuilder(INamedTypeSymbol source, INamedTypeSymbol destination, ImmutableArray<string> propertyNames,
			ContainingNamespaceKind kind, Compilation compilation, ConfigurationValues configurationValues) =>
			this.Text = MappingBuilder.Build(source, destination, propertyNames, kind, compilation, configurationValues);

		private static SourceText Build(INamedTypeSymbol source, INamedTypeSymbol destination, ImmutableArray<string> propertyNames,
			ContainingNamespaceKind kind, Compilation compilation, ConfigurationValues configurationValues)
		{
			using var writer = new StringWriter();
			using var indentWriter = new IndentedTextWriter(writer,
				configurationValues.IndentStyle == IndentStyle.Tab ? "\t" : new string(' ', (int)configurationValues.IndentSize));

			var namespaces = new NamespaceGatherer();
			var emittedNamespace = false;

			if (kind != ContainingNamespaceKind.Global)
			{
				if (kind == ContainingNamespaceKind.Source)
				{
					if (source.ContainingNamespace.IsGlobalNamespace ||
						!source.ContainingNamespace.Contains(destination.ContainingNamespace))
					{
						namespaces.Add(destination.ContainingNamespace);
					}

					if (!source.ContainingNamespace.IsGlobalNamespace)
					{
						indentWriter.WriteLine($"namespace {source.ContainingNamespace.ToDisplayString()}");
						indentWriter.WriteLine("{");
						indentWriter.Indent++;
						emittedNamespace = true;
					}
				}
				else if (kind == ContainingNamespaceKind.Destination)
				{
					if (destination.ContainingNamespace.IsGlobalNamespace ||
						!destination.ContainingNamespace.Contains(source.ContainingNamespace))
					{
						namespaces.Add(source.ContainingNamespace);
					}

					if (!destination.ContainingNamespace.IsGlobalNamespace)
					{
						indentWriter.WriteLine($"namespace {destination.ContainingNamespace.ToDisplayString()}");
						indentWriter.WriteLine("{");
						indentWriter.Indent++;
						emittedNamespace = true;
					}
				}
			}
			else
			{
				namespaces.Add(source.ContainingNamespace);
				namespaces.Add(destination.ContainingNamespace);
			}

			MappingBuilder.BuildType(source, destination, propertyNames, compilation, indentWriter, namespaces);

			if (emittedNamespace)
			{
				indentWriter.Indent--;
				indentWriter.WriteLine("}");
			}

			var code = namespaces.Values.Count > 0 ?
				string.Join(Environment.NewLine,
					string.Join(Environment.NewLine, namespaces.Values.Select(_ => $"using {_};")), 
					string.Empty, "#nullable enable", string.Empty, writer.ToString()) :
				string.Join(Environment.NewLine, "#nullable enable", string.Empty, writer.ToString());

			return SourceText.From(code, Encoding.UTF8);
		}

		private static void BuildType(INamedTypeSymbol source, INamedTypeSymbol destination, ImmutableArray<string> propertyNames, 
			Compilation compilation, IndentedTextWriter indentWriter, NamespaceGatherer namespaces)
		{
			indentWriter.WriteLine($"public static partial class {source.Name}MapToExtensions");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			var constructors = destination.Constructors.Where(_ => _.DeclaredAccessibility == Accessibility.Public ||
				destination.ContainingAssembly.ExposesInternalsTo(compilation.Assembly) && _.DeclaredAccessibility == Accessibility.Friend).ToArray();

			for (var i = 0; i < constructors.Length; i++)
			{
				var constructor = constructors[i];
				MappingBuilder.BuildMapExtensionMethod(source, destination, propertyNames, constructor, namespaces, indentWriter);

				if (i < constructors.Length - 1)
				{
					indentWriter.WriteLine();
				}
			}

			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}

		private static void BuildMapExtensionMethod(ITypeSymbol source, ITypeSymbol destination, ImmutableArray<string> propertyNames,
			IMethodSymbol constructor, NamespaceGatherer namespaces, IndentedTextWriter indentWriter)
		{
			var parameters = new string[constructor.Parameters.Length + 1];
			parameters[0] = $"this {source.Name} self";

			for (var i = 0; i < constructor.Parameters.Length; i++)
			{
				var parameter = constructor.Parameters[i];
				namespaces.Add(parameter.Type.ContainingNamespace);
				var nullableAnnotation = parameter.NullableAnnotation == NullableAnnotation.Annotated ? "?" : string.Empty;
				var optionalValue = parameter.HasExplicitDefaultValue ? $" = {parameter.ExplicitDefaultValue.GetDefaultValue()}" : string.Empty;
				parameters[i + 1] = $"{parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}{nullableAnnotation} {parameter.Name}{optionalValue}";
			}

			indentWriter.WriteLine($"public static {destination.Name} MapTo{destination.Name}({string.Join(", ", parameters)}) =>");
			indentWriter.Indent++;

			if (!source.IsValueType)
			{
				indentWriter.WriteLine("self is null ? throw new ArgumentNullException(nameof(self)) :");
				namespaces.Add(typeof(ArgumentNullException));
				indentWriter.Indent++;
			}

			if (constructor.Parameters.Length == 0)
			{
				indentWriter.WriteLine($"new {destination.Name}");
			}
			else
			{
				indentWriter.WriteLine(
					$"new {destination.Name}({string.Join(", ", constructor.Parameters.Select(_ => _.Name))})");
			}

			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			foreach (var propertyName in propertyNames)
			{
				indentWriter.WriteLine($"{propertyName} = self.{propertyName},");
			}

			indentWriter.Indent--;
			indentWriter.WriteLine("};");

			if (!source.IsValueType)
			{
				indentWriter.Indent--;
			}

			indentWriter.Indent--;
		}

		public SourceText Text { get; private set; }
	}
}