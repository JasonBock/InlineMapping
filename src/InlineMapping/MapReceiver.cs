using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace InlineMapping
{
	internal sealed class MapReceiver
		: ISyntaxReceiver
	{
		public List<AttributeSyntax> MapCandidates { get; } = new();
		public List<TypeDeclarationSyntax> MapFromCandidates { get; } = new();
		public List<TypeDeclarationSyntax> MapToCandidates { get; } = new();
		
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if(syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
			{
				foreach (var attributeList in typeDeclarationSyntax.AttributeLists)
				{
					foreach (var attribute in attributeList.Attributes)
					{
						if (attribute.Name.ToString() == nameof(MapFromAttribute).Replace(nameof(Attribute), string.Empty) ||
							attribute.Name.ToString() == nameof(MapFromAttribute))
						{
							this.MapFromCandidates.Add(typeDeclarationSyntax);
						}
						else if (attribute.Name.ToString() == nameof(MapToAttribute).Replace(nameof(Attribute), string.Empty) ||
							attribute.Name.ToString() == nameof(MapToAttribute))
						{
							this.MapToCandidates.Add(typeDeclarationSyntax);
						}
					}
				}
			}
			else if(syntaxNode is AttributeSyntax attributeSyntax)
			{
				if (attributeSyntax.Name.ToString() == nameof(MapAttribute).Replace(nameof(Attribute), string.Empty) ||
					attributeSyntax.Name.ToString() == nameof(MapAttribute))
				{
					this.MapCandidates.Add(attributeSyntax);
				}
			}
		}
	}
}