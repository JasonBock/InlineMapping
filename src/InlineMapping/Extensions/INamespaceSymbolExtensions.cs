using Microsoft.CodeAnalysis;

namespace InlineMapping.Extensions
{
	internal static class INamespaceSymbolExtensions
	{
		internal static string GetName(this INamespaceSymbol? self) =>
			self?.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) ?? string.Empty;
	}
}