using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace InlineMapping
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class UsingInlineMapperAnalyzer
		: DiagnosticAnalyzer
	{
		private static DiagnosticDescriptor descriptor =
			new DiagnosticDescriptor(
				UsingInlineMapperConstants.Id, 
				UsingInlineMapperConstants.Title,
				UsingInlineMapperConstants.Message, 
				UsingInlineMapperConstants.Category,
				DiagnosticSeverity.Error, true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(UsingInlineMapperAnalyzer.descriptor);
			}
		}

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(
				UsingInlineMapperAnalyzer.AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
		}

		private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
		{
			var invocationNode = (InvocationExpressionSyntax)context.Node;
			var invocationMethod = context.SemanticModel.GetSymbolInfo(invocationNode).Symbol as IMethodSymbol;

			if (invocationMethod != null &&
				invocationMethod.Name == nameof(InlineMapper.Map) &&
				invocationMethod.ContainingType.Name == nameof(InlineMapper) &&
				typeof(InlineMapper).AssemblyQualifiedName.Contains(invocationMethod.ContainingType.ContainingAssembly.Name))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					UsingInlineMapperAnalyzer.descriptor, invocationNode.GetLocation()));
			}
		}
	}
}