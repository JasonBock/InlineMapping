using Microsoft.CodeAnalysis;

namespace InlineMapping.Descriptors
{
	public static class NoAccessibleConstructorsDiagnostic
	{
		public static Diagnostic Create(SyntaxNode currentNode) =>
			Diagnostic.Create(new DiagnosticDescriptor(
				NoAccessibleConstructorsDiagnostic.Id, NoAccessibleConstructorsDiagnostic.Title,
				NoAccessibleConstructorsDiagnostic.Message, DescriptorConstants.Usage, DiagnosticSeverity.Error, true,
				helpLinkUri: HelpUrlBuilder.Build(
					NoAccessibleConstructorsDiagnostic.Id, NoAccessibleConstructorsDiagnostic.Title)),
				currentNode.GetLocation());

		public const string Id = "IM1";
		public const string Message = "The destination type must have at least one accessible constructor.";
		public const string Title = "Missing Accessible Constructor On Destination Type";
	}
}