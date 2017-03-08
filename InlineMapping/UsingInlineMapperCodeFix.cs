using InlineMapping.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace InlineMapping
{
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	[Shared]
	public sealed class UsingInlineMapperCodeFix
		: CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds
		{
			get
			{
				return ImmutableArray.Create(UsingInlineMapperConstants.Id);
			}
		}

		public override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

			context.CancellationToken.ThrowIfCancellationRequested();

			var diagnostic = context.Diagnostics.First();
			var invocationNode = root.FindNode(diagnostic.Location.SourceSpan) as InvocationExpressionSyntax;
			var invocationMethod = model.GetSymbolInfo(invocationNode).Symbol as IMethodSymbol;

			var invocationParent = invocationNode.Parent;
			var sourceArgument = invocationNode.ArgumentList.Arguments[0];
			var destinationArgument = invocationNode.ArgumentList.Arguments[1];

			var sourceType = invocationMethod.TypeArguments[0];
			var destinationType = invocationMethod.TypeArguments[1];
			var destinationProperties = destinationType.GetMembers().OfType<IPropertySymbol>();
			var expressions = new List<SyntaxNode>();
			var comments = new List<SyntaxTrivia>();

			foreach (var sourceProperty in sourceType.GetMembers().OfType<IPropertySymbol>().Where(_ => _.GetMethod != null))
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				var destinationProperty = destinationProperties.FirstOrDefault(
					_ => _.Name == sourceProperty.Name && _.SetMethod != null);

				if (destinationProperty != null)
				{
					if (destinationProperty.Type.IsAssignableFrom(sourceProperty.Type))
					{
						var expression = $"{destinationArgument.Expression}.{destinationProperty.Name} = {sourceArgument.Expression}.{sourceProperty.Name};{Environment.NewLine}";
						expressions.Add(SyntaxFactory.ParseStatement(expression).WithAdditionalAnnotations(Formatter.Annotation));
					}
					else
					{
						comments.Add(SyntaxFactory.Comment(
							$"// TODO - The type for the source property {sourceProperty.Name}, {sourceProperty.Type.Name}, cannot be assigned to the destination property of type {destinationProperty.Type.Name}{Environment.NewLine}")
							.WithAdditionalAnnotations(Formatter.Annotation));
					}
				}
			}

			var newRoot = UsingInlineMapperCodeFix.CreateNewRoot(
				root, invocationParent, comments, expressions);

			if(newRoot != root)
			{
				context.RegisterCodeFix(
					CodeAction.Create(
						UsingInlineMapperConstants.CodeFixTitle,
						_ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
						UsingInlineMapperConstants.CodeFixTitle), diagnostic);
			}
		}

		private static SyntaxNode CreateNewRoot(SyntaxNode root, SyntaxNode invocationParent,
			List<SyntaxTrivia> comments, List<SyntaxNode> expressions)
		{
			if (comments.Count > 0)
			{
				if (expressions.Count > 0)
				{
					comments.Insert(0, SyntaxFactory.EndOfLine(Environment.NewLine));
					expressions[expressions.Count - 1] =
						expressions[expressions.Count - 1].WithTrailingTrivia(
							SyntaxFactory.TriviaList(comments));
					return root.ReplaceNode(invocationParent, expressions);
				}
				else
				{
					comments.AddRange(invocationParent.GetTrailingTrivia().ToList());
					comments.Insert(0, SyntaxFactory.EndOfLine(Environment.NewLine));
					var newParent = invocationParent.WithTrailingTrivia(
						SyntaxFactory.TriviaList(comments));

					return root.ReplaceNode(invocationParent, newParent);
				}
			}
			else if (expressions.Count > 0)
			{
				return root.ReplaceNode(invocationParent, expressions);
			}
			else
			{
				return root;
			}
		}
	}
}