using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace InlineMapping.Extensions;

public static class AttributeSyntaxExtensions
{
	public static AttributeData? GetAttributeData(this AttributeSyntax self, SemanticModel model)
	{
		if (self is null)
		{
			throw new ArgumentNullException(nameof(self));
		}

		if (model is null)
		{
			throw new ArgumentNullException(nameof(model));
		}

		static AttributeData? FindAttributeData(AttributeSyntax self, ImmutableArray<AttributeData> attributes)
		{
			return attributes.SingleOrDefault(_ => _.ApplicationSyntaxReference!.GetSyntax() == self);
		}

		// All attributes are part of a list.
		var attributeList = self.FindParent<AttributeListSyntax>();

		// If the attribute list targets "assembly" or "module", we'll look for 
		// attribute data within the Assembly's attribute data list.
		if (attributeList is not null && attributeList.Target is not null &&
			(attributeList.Target.Identifier.ValueText == "assembly" ||
			attributeList.Target.Identifier.ValueText == "module"))
		{
			return FindAttributeData(self, model.Compilation.Assembly.GetAttributes());
		}
		else
		{
			var parent = self.Parent;

			// We'll look up the tree for specific nodes to find attribute data.
			while (parent is not null)
			{
				if (parent is FieldDeclarationSyntax ||
					parent is EventFieldDeclarationSyntax)
				{
					// Events and fields have the attribute data on a descendant
					// variable declarator node
					parent = parent.DescendantNodes(_ => true)
						.OfType<VariableDeclaratorSyntax>().First();
				}

				var parentSymbol = model.GetDeclaredSymbol(parent);

				if (parentSymbol is not null)
				{
					if (parentSymbol is IMethodSymbol methodSymbol)
					{
						// We need to check both the method's attribute data list
						// as well as its return attribute data list.
						return FindAttributeData(self, methodSymbol.GetAttributes()) ??
							FindAttributeData(self, methodSymbol.GetReturnTypeAttributes());
					}

					return FindAttributeData(self, parentSymbol.GetAttributes());
				}

				parent = parent.Parent;
			}
		}

		return null;
	}
}