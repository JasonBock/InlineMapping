using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace InlineMapping
{
	public sealed class MapToReceiver
		: ISyntaxReceiver
	{
		public List<TypeDeclarationSyntax> Candidates { get; } = new List<TypeDeclarationSyntax>();
		
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if(syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax)
			{
				foreach (var attributeList in typeDeclarationSyntax.AttributeLists)
				{
					foreach (var attribute in attributeList.Attributes)
					{
						if(attribute.Name.ToString() == "MapTo" ||
							attribute.Name.ToString() == "MapToAttribute")
						{
							this.Candidates.Add(typeDeclarationSyntax);
						}
					}
				}
			}
		}
	}
}