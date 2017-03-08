using Microsoft.CodeAnalysis;
using System.Linq;

namespace InlineMapping.Playground.Extensions
{
	internal static class ITypeSymbolExtensions
	{
		internal static bool IsAssignableFrom(this ITypeSymbol @this, ITypeSymbol other)
		{
			return @this != null &&
				other != null &&
				((@this.MetadataName == other.MetadataName &&
					@this.ContainingAssembly.MetadataName == other.ContainingAssembly.MetadataName) ||
				@this.IsAssignableFrom(other.BaseType) ||
				other.Interfaces.Any(_ => @this.IsAssignableFrom(_)));
		}
	}
}
