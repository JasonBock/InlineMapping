﻿using InlineMapping.Metadata;
using InlineMapping.Metadata.Descriptors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;

namespace InlineMapping
{
	[Generator]
	public sealed class MapToGenerator
		: ISourceGenerator
	{
		private static (ImmutableList<Diagnostic> diagnostics, string? name, SourceText? text) GenerateMapping(
			ITypeSymbol sourceType, AttributeData attributeData)
		{
			var diagnostics = ImmutableList.CreateBuilder<Diagnostic>();

			var destinationType = (INamedTypeSymbol)attributeData.ConstructorArguments[0].Value!;

			if (!destinationType.Constructors.Any(_ => _.DeclaredAccessibility == Accessibility.Public && _.Parameters.Length == 0))
			{
				diagnostics.Add(Diagnostic.Create(new DiagnosticDescriptor(
					NoArgumentConstructorDescriptorConstants.Id, NoArgumentConstructorDescriptorConstants.Title,
					NoArgumentConstructorDescriptorConstants.Message, DescriptorConstants.Usage, DiagnosticSeverity.Error, true,
					helpLinkUri: HelpUrlBuilder.Build(
						NoArgumentConstructorDescriptorConstants.Id, NoArgumentConstructorDescriptorConstants.Title)),
					attributeData.ApplicationSyntaxReference!.GetSyntax().GetLocation()));
			}

			var maps = new List<string>();

			var destinationProperties = destinationType.GetMembers().OfType<IPropertySymbol>()
				.Where(_ => _.DeclaredAccessibility == Accessibility.Public && _.SetMethod is not null).ToList();

			if(destinationProperties.Count > 0)
			{
				foreach (var sourceProperty in sourceType.GetMembers().OfType<IPropertySymbol>()
					.Where(_ => _.DeclaredAccessibility == Accessibility.Public && _.GetMethod is not null))
				{
					var destinationProperty = destinationProperties.FirstOrDefault(
						_ => _.Name == sourceProperty.Name &&
							_.Type.Equals(sourceProperty.Type, SymbolEqualityComparer.Default) &&
							(sourceProperty.NullableAnnotation != NullableAnnotation.Annotated ||
								sourceProperty.NullableAnnotation == NullableAnnotation.Annotated && _.NullableAnnotation == NullableAnnotation.Annotated));

					if (destinationProperty is not null)
					{
						maps.Add($"\t\t\t\t\t{destinationProperty.Name} = self.{sourceProperty.Name},");
						destinationProperties.Remove(destinationProperty);
					}
					else
					{
						diagnostics.Add(Diagnostic.Create(new DiagnosticDescriptor(
							NoMatchDescriptorConstants.Id, NoMatchDescriptorConstants.Title,
							string.Format(CultureInfo.CurrentCulture, NoMatchDescriptorConstants.Message, sourceProperty.Name, "source", sourceType.Name), 
							DescriptorConstants.Usage, DiagnosticSeverity.Info, true,
							helpLinkUri: HelpUrlBuilder.Build(
								NoMatchDescriptorConstants.Id, NoMatchDescriptorConstants.Title)), null));
					}
				}
			}

			foreach(var remainingDestinationProperty in destinationProperties)
			{
				diagnostics.Add(Diagnostic.Create(new DiagnosticDescriptor(
					NoMatchDescriptorConstants.Id, NoMatchDescriptorConstants.Title,
					string.Format(CultureInfo.CurrentCulture, NoMatchDescriptorConstants.Message, remainingDestinationProperty.Name, "destination", destinationType.Name),
					DescriptorConstants.Usage, DiagnosticSeverity.Info, true,
					helpLinkUri: HelpUrlBuilder.Build(
						NoMatchDescriptorConstants.Id, NoMatchDescriptorConstants.Title)), null));
			}

			if (maps.Count == 0)
			{
				diagnostics.Add(Diagnostic.Create(new DiagnosticDescriptor(
					NoPropertyMapsFoundDescriptorConstants.Id, NoPropertyMapsFoundDescriptorConstants.Title,
					NoPropertyMapsFoundDescriptorConstants.Message, DescriptorConstants.Usage, DiagnosticSeverity.Error, true,
					helpLinkUri: HelpUrlBuilder.Build(
						NoPropertyMapsFoundDescriptorConstants.Id, NoPropertyMapsFoundDescriptorConstants.Title)),
					attributeData.ApplicationSyntaxReference!.GetSyntax().GetLocation()));
			}

			if(!diagnostics.Any(_ => _.Severity == DiagnosticSeverity.Error))
			{
				// TODO: If the namespace is the same as one of the usings, or it's "in" one of them, 
				// we probably don't need to declare the namespace explicitly then.
				var usingStatements = new SortedSet<string>
				{
					"using System;"
				};

				if (!destinationType.ContainingNamespace.IsGlobalNamespace)
				{
					usingStatements.Add($"using {destinationType.ContainingNamespace.ToDisplayString()};");
				}

				var namespaceStartingStatement = !sourceType.ContainingNamespace.IsGlobalNamespace ?
					$"namespace {sourceType.ContainingNamespace.ToDisplayString()}{Environment.NewLine}{{{Environment.NewLine}" : string.Empty;
				var namespaceEndingStatement = !sourceType.ContainingNamespace.IsGlobalNamespace ?
					$"{Environment.NewLine}}}" : string.Empty;

				var performNullCheck = !sourceType.IsValueType ?
					$"\t\t\tself is null ? throw new ArgumentNullException(nameof(self)) :{Environment.NewLine}" : string.Empty;

				var text = SourceText.From(
	@$"{string.Join(Environment.NewLine, usingStatements)}{Environment.NewLine}{Environment.NewLine}{namespaceStartingStatement}	public static partial class {sourceType.Name}MapToExtensions
	{{
		public static {destinationType.Name} MapTo{destinationType.Name}(this {sourceType.Name} self) =>
{performNullCheck}				new {destinationType.Name}
				{{
{string.Join(Environment.NewLine, maps)}
				}};
	}}{namespaceEndingStatement}", Encoding.UTF8);
				return (diagnostics.ToImmutableList(), $"{sourceType.Name}_To_{destinationType.Name}_Map.g.cs", text);
			}

			return (diagnostics.ToImmutableList(), null, null);
		}

		public void Execute(SourceGeneratorContext context)
		{
			if (context.SyntaxReceiver is MapToReceiver receiver)
			{
				var compilation = context.Compilation;
				var mapToSymbol = compilation.GetTypeByMetadataName(typeof(MapToAttribute).FullName);

				foreach (var candidateTypeNode in receiver.Candidates)
				{
					var model = compilation.GetSemanticModel(candidateTypeNode.SyntaxTree);
					var candidateTypeSymbol = model.GetDeclaredSymbol(candidateTypeNode) as ITypeSymbol;

					if (candidateTypeSymbol is not null)
					{
						foreach (var mappingAttribute in candidateTypeSymbol.GetAttributes().Where(
							_ => _.AttributeClass!.Equals(mapToSymbol, SymbolEqualityComparer.Default)))
						{
							var (diagnostics, name, text) = MapToGenerator.GenerateMapping(candidateTypeSymbol, mappingAttribute);

							foreach(var diagnostic in diagnostics)
							{
								context.ReportDiagnostic(diagnostic);
							}

							if (name is not null && text is not null)
							{
								context.AddSource(name, text);
							}
						}
					}
				}
			}
		}

		public void Initialize(InitializationContext context) => context.RegisterForSyntaxNotifications(() => new MapToReceiver());
	}
}