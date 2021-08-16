using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace InlineMapping
{
	internal sealed class MapReceiver
		: ISyntaxContextReceiver
	{
		public List<(INamedTypeSymbol source, INamedTypeSymbol destination, SyntaxNode origination, MappingContext context)> Targets { get; } = new();

		public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
		{
			var syntaxNode = context.Node;
			var model = context.SemanticModel;

			if (syntaxNode is TypeDeclarationSyntax)
			{
				var mapFromAttributeSymbol = model.Compilation.GetTypeByMetadataName(typeof(MapFromAttribute).FullName);
				var mapToAttributeSymbol = model.Compilation.GetTypeByMetadataName(typeof(MapToAttribute).FullName);
				var typeSymbol = model.GetDeclaredSymbol(syntaxNode) as INamedTypeSymbol;

				if (typeSymbol is not null)
				{
					foreach (var typeAttribute in typeSymbol.GetAttributes())
					{
						if (SymbolEqualityComparer.Default.Equals(typeAttribute.AttributeClass!, mapToAttributeSymbol))
						{
							this.Targets.Add((typeSymbol, (INamedTypeSymbol)typeAttribute.ConstructorArguments[0].Value!, syntaxNode, 
								new MappingContext((ContainingNamespaceKind)typeAttribute.ConstructorArguments[1].Value!, 
									(MatchingPropertyTypeKind)typeAttribute.ConstructorArguments[2].Value!)));
						}
						else if (SymbolEqualityComparer.Default.Equals(typeAttribute.AttributeClass!, mapFromAttributeSymbol))
						{
							this.Targets.Add(((INamedTypeSymbol)typeAttribute.ConstructorArguments[0].Value!, typeSymbol, syntaxNode, 
								new MappingContext((ContainingNamespaceKind)typeAttribute.ConstructorArguments[1].Value!,
									(MatchingPropertyTypeKind)typeAttribute.ConstructorArguments[2].Value!)));
						}
					}
				}
			}
			else if(syntaxNode is AttributeSyntax)
			{
				var mapAttributeSymbol = model.Compilation.GetTypeByMetadataName(typeof(MapAttribute).FullName);
				var symbol = model.GetSymbolInfo(syntaxNode).Symbol!.ContainingSymbol;

				if(SymbolEqualityComparer.Default.Equals(symbol, mapAttributeSymbol))
				{
					var attributeData = model.Compilation.Assembly.GetAttributes().Single(
						_ => _.ApplicationSyntaxReference!.GetSyntax() == syntaxNode);
					var sourceType = (INamedTypeSymbol)attributeData.ConstructorArguments[0].Value!;
					var destinationType = (INamedTypeSymbol)attributeData.ConstructorArguments[1].Value!;
					this.Targets.Add((sourceType, destinationType, syntaxNode, 
						new MappingContext((ContainingNamespaceKind)attributeData.ConstructorArguments[2].Value!,
							(MatchingPropertyTypeKind)attributeData.ConstructorArguments[3].Value!)));
				}
			}
		}
	}
}