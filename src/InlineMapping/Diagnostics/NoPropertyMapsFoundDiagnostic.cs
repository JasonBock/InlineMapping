using Microsoft.CodeAnalysis;

namespace InlineMapping.Diagnostics;

internal static class NoPropertyMapsFoundDiagnostic
{
   internal static Diagnostic Create(SyntaxNode currentNode) =>
	   Diagnostic.Create(new DiagnosticDescriptor(
		   NoPropertyMapsFoundDiagnostic.Id, NoPropertyMapsFoundDiagnostic.Title,
		   NoPropertyMapsFoundDiagnostic.Message, DescriptorConstants.Usage, DiagnosticSeverity.Error, true,
		   helpLinkUri: HelpUrlBuilder.Build(
			   NoPropertyMapsFoundDiagnostic.Id, NoPropertyMapsFoundDiagnostic.Title)),
		   currentNode.GetLocation());

   internal const string Id = "IM2";
   internal const string Message = "There were no properties found between the source and destination types to map.";
   internal const string Title = "No Property Maps Found";
}