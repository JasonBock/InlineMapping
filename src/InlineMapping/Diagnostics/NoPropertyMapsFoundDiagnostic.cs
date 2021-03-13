using Microsoft.CodeAnalysis;

namespace InlineMapping.Descriptors
{
	public static class NoPropertyMapsFoundDiagnostic
	{
		public static Diagnostic Create(SyntaxNode currentNode) =>
			Diagnostic.Create(new DiagnosticDescriptor(
				NoPropertyMapsFoundDiagnostic.Id, NoPropertyMapsFoundDiagnostic.Title,
				NoPropertyMapsFoundDiagnostic.Message, DescriptorConstants.Usage, DiagnosticSeverity.Error, true,
				helpLinkUri: HelpUrlBuilder.Build(
					NoPropertyMapsFoundDiagnostic.Id, NoPropertyMapsFoundDiagnostic.Title)),
				currentNode.GetLocation());

		public const string Id = "IM2";
		public const string Message = "There were no properties found between the source and destination types to map.";
		public const string Title = "No Property Maps Found";
	}
}