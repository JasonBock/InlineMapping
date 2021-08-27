using InlineMapping.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace InlineMapping
{
	internal sealed class NamespaceGatherer
	{
		private readonly ImmutableHashSet<string>.Builder builder =
			ImmutableHashSet.CreateBuilder<string>();

		public void Add(INamespaceSymbol @namespace)
		{
			if(!@namespace.IsGlobalNamespace)
			{
				this.builder.Add(@namespace.GetName());
			}
		}

		public void Add(Type type)
		{
			if(!string.IsNullOrWhiteSpace(type.Namespace))
			{
				this.builder.Add(type.Namespace);
			}
		}

		public void AddRange(IEnumerable<INamespaceSymbol> namespaces)
		{
			foreach(var @namespace in namespaces)
			{
				this.Add(@namespace);
			}
		}

		public IImmutableSet<string> Values => this.builder.ToImmutableSortedSet();
	}
}