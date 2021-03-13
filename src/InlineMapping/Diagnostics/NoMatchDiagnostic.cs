using Microsoft.CodeAnalysis;
using System.Globalization;

namespace InlineMapping.Descriptors
{
	public static class NoMatchDiagnostic
	{
		public static Diagnostic Create(IPropertySymbol property, string targetName, INamedTypeSymbol target) =>
			Diagnostic.Create(new DiagnosticDescriptor(
				NoMatchDiagnostic.Id, NoMatchDiagnostic.Title,
				string.Format(CultureInfo.CurrentCulture, NoMatchDiagnostic.Message, property.Name, targetName, target.Name),
				DescriptorConstants.Usage, DiagnosticSeverity.Info, true,
				helpLinkUri: HelpUrlBuilder.Build(
					NoMatchDiagnostic.Id, NoMatchDiagnostic.Title)), null);

		public const string Id = "IM3";
		public const string Message = "A match for {0} on the {1} type {2} could not be found.";
		public const string Title = "No Property Match Found";
	}
}