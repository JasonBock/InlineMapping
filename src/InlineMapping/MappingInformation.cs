using InlineMapping.Descriptors;
using InlineMapping.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using Maps = System.Collections.Immutable.ImmutableDictionary<(Microsoft.CodeAnalysis.INamedTypeSymbol source, Microsoft.CodeAnalysis.INamedTypeSymbol destination),
	(System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.Diagnostic> diagnostics, Microsoft.CodeAnalysis.SyntaxNode node, System.Collections.Immutable.ImmutableArray<string> maps, InlineMapping.ContainingNamespaceKind kind)>;

namespace InlineMapping
{
	internal sealed class MappingInformation
	{
		public MappingInformation(MapReceiver receiver, Compilation compilation) =>
			this.Maps = MappingInformation.Validate(receiver, compilation);

		private static Maps Validate(MapReceiver receiver, Compilation compilation)
		{
			var maps = ImmutableDictionary.CreateBuilder<
				(INamedTypeSymbol, INamedTypeSymbol), (ImmutableArray<Diagnostic>, SyntaxNode, ImmutableArray<string>, ContainingNamespaceKind)>();

			foreach(var (source, destination, origination, kind) in receiver.Targets)
			{
				MappingInformation.ValidatePairs(origination, source, destination, maps, kind, compilation);
			}

			return maps.ToImmutable();
		}

		private static void ValidatePairs(SyntaxNode currentNode, INamedTypeSymbol source, INamedTypeSymbol destination,
			Maps.Builder maps, ContainingNamespaceKind kind, Compilation compilation)
		{
			var key = (source, destination);

			if (maps.ContainsKey(key))
			{
				var diagnostics = maps[key].diagnostics.ToList();
				diagnostics.Add(DuplicatedAttributeDiagnostic.Create(currentNode, maps[key].node));
				maps[key] = (diagnostics.ToImmutableArray(), maps[key].node, maps[key].maps, kind);
			}
			else
			{
				var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

				if (!destination.Constructors.Any(_ => _.DeclaredAccessibility == Accessibility.Public ||
					destination.ContainingAssembly.ExposesInternalsTo(compilation.Assembly) && _.DeclaredAccessibility == Accessibility.Friend))
				{
					diagnostics.Add(NoAccessibleConstructorsDiagnostic.Create(currentNode));
				}

				var propertyMaps = ImmutableArray.CreateBuilder<string>();

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
							_.Type.Equals(sourceProperty.Type, SymbolEqualityComparer.Default) &&
							(sourceProperty.NullableAnnotation != NullableAnnotation.Annotated ||
								sourceProperty.NullableAnnotation == NullableAnnotation.Annotated && _.NullableAnnotation == NullableAnnotation.Annotated));

					if (destinationProperty is not null)
					{
						propertyMaps.Add($"{destinationProperty.Name} = self.{sourceProperty.Name},");
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

				if (propertyMaps.Count == 0)
				{
					diagnostics.Add(NoPropertyMapsFoundDiagnostic.Create(currentNode));
				}

				maps.Add((source, destination), (diagnostics.ToImmutable(), currentNode, propertyMaps.ToImmutable(), kind));
			}
		}

		public Maps Maps { get; }
	}
}