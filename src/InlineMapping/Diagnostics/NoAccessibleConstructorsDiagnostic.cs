using Microsoft.CodeAnalysis;

namespace InlineMapping.Diagnostics;

internal static class NoAccessibleConstructorsDiagnostic
{
   internal static Diagnostic Create(SyntaxNode currentNode) =>
	   Diagnostic.Create(new DiagnosticDescriptor(
		   NoAccessibleConstructorsDiagnostic.Id, NoAccessibleConstructorsDiagnostic.Title,
		   NoAccessibleConstructorsDiagnostic.Message, DescriptorConstants.Usage, DiagnosticSeverity.Error, true,
		   helpLinkUri: HelpUrlBuilder.Build(
			   NoAccessibleConstructorsDiagnostic.Id, NoAccessibleConstructorsDiagnostic.Title)),
		   currentNode.GetLocation());

   internal const string Id = "IM1";
   internal const string Message = "The destination type must have at least one accessible constructor.";
   internal const string Title = "Missing Accessible Constructor On Destination Type";
}