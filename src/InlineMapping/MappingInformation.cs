using InlineMapping.Descriptors;
using InlineMapping.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using Maps = System.Collections.Immutable.ImmutableDictionary<(Microsoft.CodeAnalysis.ITypeSymbol source, Microsoft.CodeAnalysis.ITypeSymbol destination),
	(System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.Diagnostic> diagnostics, Microsoft.CodeAnalysis.SyntaxNode node, System.Collections.Immutable.ImmutableArray<string> maps)>;

namespace InlineMapping
{
	internal sealed class MappingInformation
	{
		public MappingInformation(MapReceiver receiver, Compilation compilation) =>
			this.Maps = this.Validate(receiver, compilation);

		private static void ValidatePairs(SyntaxNode currentNode, INamedTypeSymbol source, INamedTypeSymbol destination, 
			Maps.Builder maps)
		{
			var key = (source, destination);

			if (maps.ContainsKey(key))
			{
				var diagnostics = maps[key].diagnostics.ToList();
				diagnostics.Add(DuplicatedAttributeDiagnostic.Create(currentNode, maps[key].node));
				maps[key] = (diagnostics.ToImmutableArray(), maps[key].node, maps[key].maps);
			}
			else
			{
				var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

				if (!destination.Constructors.Any(_ => _.DeclaredAccessibility == Accessibility.Public && _.Parameters.Length == 0))
				{
					diagnostics.Add(NoArgumentConstructorDiagnostic.Create(currentNode));
				}

				var propertyMaps = ImmutableArray.CreateBuilder<string>();

				var destinationProperties = destination.GetMembers().OfType<IPropertySymbol>()
					.Where(_ => _.SetMethod is not null && 
						(_.SetMethod.DeclaredAccessibility == Accessibility.Public ||
						(destination.ContainingAssembly.ExposesInternalsTo(source.ContainingAssembly) && _.SetMethod.DeclaredAccessibility == Accessibility.Friend)))
					.ToList();

				foreach (var sourceProperty in source.GetMembers().OfType<IPropertySymbol>()
					.Where(_ => _.GetMethod is not null && 
						(_.GetMethod.DeclaredAccessibility == Accessibility.Public ||
						(source.ContainingAssembly.ExposesInternalsTo(destination.ContainingAssembly) && _.GetMethod.DeclaredAccessibility == Accessibility.Friend))))
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

				maps.Add((source, destination), (diagnostics.ToImmutable(), currentNode, propertyMaps.ToImmutable()));
			}
		}

		private Maps Validate(MapReceiver receiver, Compilation compilation)
		{
			var maps = ImmutableDictionary.CreateBuilder<(ITypeSymbol, ITypeSymbol), (ImmutableArray<Diagnostic>, SyntaxNode, ImmutableArray<string>)>();

			foreach(var target in receiver.Targets)
			{
				MappingInformation.ValidatePairs(target.origination, target.source, target.destination, maps);
			}

			return maps.ToImmutable();
		}

		public Maps Maps { get; }
	}
}