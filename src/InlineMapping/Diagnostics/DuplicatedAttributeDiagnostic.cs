using Microsoft.CodeAnalysis;

namespace InlineMapping.Descriptors
{
	public static class DuplicatedAttributeDiagnostic
	{
		public static Diagnostic Create(SyntaxNode currentNode, SyntaxNode previousNode) =>
			Diagnostic.Create(new DiagnosticDescriptor(
				DuplicatedAttributeDiagnostic.Id, DuplicatedAttributeDiagnostic.Title,
				DuplicatedAttributeDiagnostic.Message, DescriptorConstants.Usage, DiagnosticSeverity.Warning, true,
				helpLinkUri: HelpUrlBuilder.Build(
					DuplicatedAttributeDiagnostic.Id, DuplicatedAttributeDiagnostic.Title)),
				currentNode.GetLocation(), new[] { previousNode });

		public const string Id = "IM4";
		public const string Message = "The source and destination types have already been mapped.";
		public const string Title = "Duplicated Mapping";
	}
}