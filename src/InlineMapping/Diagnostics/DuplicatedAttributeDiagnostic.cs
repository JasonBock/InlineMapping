using Microsoft.CodeAnalysis;

namespace InlineMapping.Diagnostics;

internal static class DuplicatedAttributeDiagnostic
{
   internal static Diagnostic Create(SyntaxNode currentNode, SyntaxNode previousNode) =>
	   Diagnostic.Create(new DiagnosticDescriptor(
		   DuplicatedAttributeDiagnostic.Id, DuplicatedAttributeDiagnostic.Title,
		   DuplicatedAttributeDiagnostic.Message, DescriptorConstants.Usage, DiagnosticSeverity.Warning, true,
		   helpLinkUri: HelpUrlBuilder.Build(
			   DuplicatedAttributeDiagnostic.Id, DuplicatedAttributeDiagnostic.Title)),
		   currentNode.GetLocation(), new[] { previousNode });

   internal const string Id = "IM4";
   internal const string Message = "The source and destination types have already been mapped.";
   internal const string Title = "Duplicated Mapping";
}