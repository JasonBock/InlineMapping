﻿using InlineMapping.Descriptors;
using InlineMapping.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace InlineMapping
{
	using Maps = ImmutableDictionary<(INamedTypeSymbol source, INamedTypeSymbol destination),
		(ImmutableArray<Diagnostic> diagnostics, SyntaxNode node, ImmutableArray<string> propertyNames, MappingContext context)>;

	internal sealed class MappingInformation
	{
		public MappingInformation(MapReceiver receiver, Compilation compilation) =>
			this.Maps = MappingInformation.Validate(receiver, compilation);

		private static Maps Validate(MapReceiver receiver, Compilation compilation)
		{
			var maps = ImmutableDictionary.CreateBuilder<
				(INamedTypeSymbol, INamedTypeSymbol), (ImmutableArray<Diagnostic>, SyntaxNode, ImmutableArray<string>, MappingContext)>();

			foreach(var (source, destination, origination, context) in receiver.Targets)
			{
				MappingInformation.ValidatePairs(origination, source, destination, maps, context, compilation);
			}

			return maps.ToImmutable();
		}

		private static void ValidatePairs(SyntaxNode currentNode, INamedTypeSymbol source, INamedTypeSymbol destination,
			Maps.Builder maps, MappingContext context, Compilation compilation)
		{
			var key = (source, destination);

			if (maps.ContainsKey(key))
			{
				var diagnostics = maps[key].diagnostics.ToList();
				diagnostics.Add(DuplicatedAttributeDiagnostic.Create(currentNode, maps[key].node));
				maps[key] = (diagnostics.ToImmutableArray(), maps[key].node, maps[key].propertyNames, context);
			}
			else
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
					var x = compilation.ClassifyCommonConversion(sourceProperty.Type, sourceProperty.Type);

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

				maps.Add((source, destination), (diagnostics.ToImmutable(), currentNode, propertyNames.ToImmutable(),
					context));
			}
		}

		public Maps Maps { get; }
	}
}