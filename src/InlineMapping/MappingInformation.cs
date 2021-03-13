using InlineMapping.Descriptors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
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

		private static void ValidateMapFrom(List<TypeDeclarationSyntax> mapFromCandidates, Maps.Builder maps, Compilation compilation) 
		{
			var mapFromAttributeSymbol = compilation.GetTypeByMetadataName(typeof(MapFromAttribute).FullName);

			foreach (var candidateTypeNode in mapFromCandidates)
			{
				var model = compilation.GetSemanticModel(candidateTypeNode.SyntaxTree);
				var destinationType = model.GetDeclaredSymbol(candidateTypeNode) as INamedTypeSymbol;

				if (destinationType is not null)
				{
					foreach (var mappingAttribute in destinationType.GetAttributes().Where(
						_ => _.AttributeClass!.Equals(mapFromAttributeSymbol, SymbolEqualityComparer.Default)))
					{
						var sourceType = (INamedTypeSymbol)mappingAttribute.ConstructorArguments[0].Value!;
						MappingInformation.ValidatePairs(mappingAttribute.ApplicationSyntaxReference!.GetSyntax(),
							sourceType, destinationType, maps);
					}
				}
			}
		}

		private static void ValidateMapTo(List<TypeDeclarationSyntax> mapToCandidates, Maps.Builder maps, Compilation compilation) 
		{
			var mapToAttributeSymbol = compilation.GetTypeByMetadataName(typeof(MapToAttribute).FullName);

			foreach (var candidateTypeNode in mapToCandidates)
			{
				var model = compilation.GetSemanticModel(candidateTypeNode.SyntaxTree);
				var sourceType = model.GetDeclaredSymbol(candidateTypeNode) as INamedTypeSymbol;

				if (sourceType is not null)
				{
					foreach (var mappingAttribute in sourceType.GetAttributes().Where(
						_ => _.AttributeClass!.Equals(mapToAttributeSymbol, SymbolEqualityComparer.Default)))
					{
						var destinationType = (INamedTypeSymbol)mappingAttribute.ConstructorArguments[0].Value!;
						MappingInformation.ValidatePairs(mappingAttribute.ApplicationSyntaxReference!.GetSyntax(), 
							sourceType, destinationType, maps);							
					}
				}
			}
		}

		private static void ValidateMap(List<AttributeSyntax> mapCandidates, Maps.Builder maps, Compilation compilation) 
		{
			var mapAttributeSymbol = compilation.GetTypeByMetadataName(typeof(MapAttribute).FullName);

			foreach (var candidateAttributeNode in mapCandidates)
			{
				var attributeSymbol = compilation.GetSemanticModel(candidateAttributeNode.SyntaxTree)
					.GetSymbolInfo(candidateAttributeNode).Symbol!.ContainingSymbol;

				if (attributeSymbol.Equals(mapAttributeSymbol, SymbolEqualityComparer.Default))
				{
					var attributeData = compilation.Assembly.GetAttributes().Single(
						_ => _.ApplicationSyntaxReference!.GetSyntax() == candidateAttributeNode);
					var sourceType = (INamedTypeSymbol)attributeData.ConstructorArguments[0].Value!;
					var destinationType = (INamedTypeSymbol)attributeData.ConstructorArguments[1].Value!;
					MappingInformation.ValidatePairs(candidateAttributeNode, sourceType, destinationType, maps);
				}
			}
		}

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
					.Where(_ => _.SetMethod is not null && _.SetMethod.DeclaredAccessibility == Accessibility.Public).ToList();

				foreach (var sourceProperty in source.GetMembers().OfType<IPropertySymbol>()
					.Where(_ => _.GetMethod is not null && _.GetMethod.DeclaredAccessibility == Accessibility.Public))
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

			MappingInformation.ValidateMapTo(receiver.MapToCandidates, maps, compilation);
			MappingInformation.ValidateMapFrom(receiver.MapFromCandidates, maps, compilation);
			MappingInformation.ValidateMap(receiver.MapCandidates, maps, compilation);

			return maps.ToImmutable();
		}

		public Maps Maps { get; }
	}
}