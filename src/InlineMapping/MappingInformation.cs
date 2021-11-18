using InlineMapping.Diagnostics;
using InlineMapping.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace InlineMapping;

internal sealed class MappingInformation
{
	public MappingInformation(SyntaxNode currentNode, INamedTypeSymbol source, INamedTypeSymbol destination,
		MappingContext context, Compilation compilation)
	{
		this.ValidatePairs(
			currentNode, source, destination, context, compilation);
		(this.Node, this.Source, this.Destination) = (currentNode, source, destination);
	}

	private void ValidatePairs(SyntaxNode currentNode, INamedTypeSymbol source, INamedTypeSymbol destination,
		MappingContext context, Compilation compilation)
	{
		var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

		if (!destination.Constructors.Any(_ => _.DeclaredAccessibility == Accessibility.Public ||
			destination.ContainingAssembly.ExposesInternalsTo(compilation.Assembly) && _.DeclaredAccessibility == Accessibility.Friend))
		{
			diagnostics.Add(NoAccessibleConstructorsDiagnostic.Create(currentNode));
		}

		var propertyNames = ImmutableArray.CreateBuilder<string>();

		var destinationProperties = destination.GetMembers().OfType<IPropertySymbol>()
			.Where(_ => _.SetMethod is not null &&
				(_.SetMethod.DeclaredAccessibility == Accessibility.Public ||
				(destination.ContainingAssembly.ExposesInternalsTo(compilation.Assembly) && _.SetMethod.DeclaredAccessibility == Accessibility.Friend)))
			.ToList();

		foreach (var sourceProperty in source.GetMembers().OfType<IPropertySymbol>()
			.Where(_ => _.GetMethod is not null &&
				(_.GetMethod.DeclaredAccessibility == Accessibility.Public ||
				(source.ContainingAssembly.ExposesInternalsTo(compilation.Assembly) && _.GetMethod.DeclaredAccessibility == Accessibility.Friend))))
		{
			var destinationProperty = destinationProperties.FirstOrDefault(
				_ => _.Name == sourceProperty.Name &&
					context.MatchingPropertyTypeKind switch
					{
						MatchingPropertyTypeKind.Exact => _.Type.Equals(sourceProperty.Type, SymbolEqualityComparer.Default),
						_ => compilation.ClassifyCommonConversion(sourceProperty.Type, _.Type).IsImplicit
					} &&
					(sourceProperty.NullableAnnotation != NullableAnnotation.Annotated ||
						sourceProperty.NullableAnnotation == NullableAnnotation.Annotated && _.NullableAnnotation == NullableAnnotation.Annotated));

			if (destinationProperty is not null)
			{
				propertyNames.Add(destinationProperty.Name);
				destinationProperties.Remove(destinationProperty);
			}
			else
			{
				diagnostics.Add(NoMatchDiagnostic.Create(sourceProperty, "source", source));
			}
		}

		foreach (var remainingDestinationProperty in destinationProperties)
		{
			diagnostics.Add(NoMatchDiagnostic.Create(remainingDestinationProperty, "destination", destination));
		}

		if (propertyNames.Count == 0)
		{
			diagnostics.Add(NoPropertyMapsFoundDiagnostic.Create(currentNode));
		}

		this.Diagnostics = diagnostics.ToImmutable();
		this.PropertyNames = propertyNames.ToImmutable();
	}

	public INamedTypeSymbol Destination { get; }
	public ImmutableArray<Diagnostic> Diagnostics { get; private set; }
	public SyntaxNode Node { get; }
	public ImmutableArray<string> PropertyNames { get; private set; }
	public INamedTypeSymbol Source { get; }
}