using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace InlineMapping
{
	internal sealed class MapToReceiver
		: ISyntaxReceiver
	{
		public List<TypeDeclarationSyntax> Candidates { get; } = new();
		
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
			{
				foreach (var attributeList 
					in typeDeclarationSyntax.AttributeLists.SelectMany(
						attributeList =>
							attributeList.Attributes
							.Select(attribute => attribute.Name.ToString())
							.Where(
								attributeName =>
									attributeName == "MapTo" || attributeName == "MapToAttribute")))
				{
					this.Candidates.Add(typeDeclarationSyntax);
				}
			}
		}
	}
}