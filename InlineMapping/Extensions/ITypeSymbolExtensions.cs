using Microsoft.CodeAnalysis;
using System.Linq;

namespace InlineMapping.Extensions
{
	internal static class ITypeSymbolExtensions
	{
		internal static bool IsAssignableFrom(this ITypeSymbol? @this, ITypeSymbol? other) =>
			@this is not null && other is not null &&
				((@this.MetadataName == other.MetadataName &&
					@this.ContainingAssembly.MetadataName == other.ContainingAssembly.MetadataName) ||
				@this.IsAssignableFrom(other.BaseType) ||
				other.Interfaces.Any(_ => @this.IsAssignableFrom(_)));
	}
}