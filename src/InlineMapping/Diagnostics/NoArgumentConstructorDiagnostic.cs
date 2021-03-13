using Microsoft.CodeAnalysis;

namespace InlineMapping.Descriptors
{
	public static class NoArgumentConstructorDiagnostic
	{
		public static Diagnostic Create(SyntaxNode currentNode) =>
			Diagnostic.Create(new DiagnosticDescriptor(
				NoArgumentConstructorDiagnostic.Id, NoArgumentConstructorDiagnostic.Title,
				NoArgumentConstructorDiagnostic.Message, DescriptorConstants.Usage, DiagnosticSeverity.Error, true,
				helpLinkUri: HelpUrlBuilder.Build(
					NoArgumentConstructorDiagnostic.Id, NoArgumentConstructorDiagnostic.Title)),
				currentNode.GetLocation());

		public const string Id = "IM1";
		public const string Message = "The destination type must have a public no-argument constructor.";
		public const string Title = "Missing No-Argument Constructor On Destination Type";
	}
}