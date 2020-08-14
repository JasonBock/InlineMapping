using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace InlineMapping
{
	public sealed class MapToReceiver
		: ISyntaxReceiver
	{
		public List<ClassDeclarationSyntax> Candidates { get; } = new List<ClassDeclarationSyntax>();
		
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if(syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
			{
				foreach (var attributeList in classDeclarationSyntax.AttributeLists)
				{
					foreach (var attribute in attributeList.Attributes)
					{
						if(attribute.Name.ToString() == "MapTo" ||
							attribute.Name.ToString() == "MapToAttribute")
						{
							this.Candidates.Add(classDeclarationSyntax);
						}
					}
				}
			}
		}
	}
}