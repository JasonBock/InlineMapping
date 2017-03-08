using System;

namespace InlineMapping.Playground
{
	public static class InlineMapper
	{
		public static void Map<TSource, TDestination>(TSource source, TDestination destination)
		{
			throw new NotSupportedException();
		}

		public static TDestination MapTo<TSource, TDestination>(this TSource @this)
		{
			throw new NotSupportedException();
		}
	}
}
