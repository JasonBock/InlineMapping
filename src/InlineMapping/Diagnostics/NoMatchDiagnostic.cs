using Microsoft.CodeAnalysis;
using System.Globalization;

namespace InlineMapping.Diagnostics;

internal static class NoMatchDiagnostic
{
   internal static Diagnostic Create(IPropertySymbol property, string targetName, INamedTypeSymbol target) =>
	   Diagnostic.Create(new DiagnosticDescriptor(
		   NoMatchDiagnostic.Id, NoMatchDiagnostic.Title,
		   string.Format(CultureInfo.CurrentCulture, NoMatchDiagnostic.Message, property.Name, targetName, target.Name),
		   DescriptorConstants.Usage, DiagnosticSeverity.Info, true,
		   helpLinkUri: HelpUrlBuilder.Build(
			   NoMatchDiagnostic.Id, NoMatchDiagnostic.Title)), null);

   internal const string Id = "IM3";
   internal const string Message = "A match for {0} on the {1} type {2} could not be found.";
   internal const string Title = "No Property Match Found";
}