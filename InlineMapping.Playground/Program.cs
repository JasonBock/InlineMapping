using InlineMapping.Playground.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InlineMapping.Playground
{
	class Program
	{
		//private static Source fieldSource;
		//private static Destination fieldDestination;

		public class Source
		{
			public int A { get; set; }
			public int B { get; set; }
		}

		public class Destination
		{
			public int A { get; set; }
			public int B { get; set; }
		}

		static void Main(string[] args)
		{
			var s = new Source();
			var d = new Destination();
			d.A = s.A;
			d.B = s.B;
			// TODO - No destination property could be found for source property C
			// TODO - No destination property could be found for source property D
			AsyncContext.Run(Program.MainAsync);
		}

		static async Task MainAsync()
		{
			await Program.ParseSimpleMapAsync();
		}

		private static async Task ParseSimpleMapAsync()
		{
			var code = @"
using InlineMapping.Playground;
using System;

public static class Test
{
	public class Source
	{
		public int One { get; set; }
		public string Two { get; set; }
		public Guid Three { get; set; }
	}
	
	public class Destination
	{
		public int One { get; set; }
		public string Two { get; set; }
		public Guid Three { get; set; }
	}

	public static void Run()
	{
		var s = new Source { One = 22, Two = ""two"", Three = Guid.NewGuid() };
		var d = new Destination();
		InlineMapper.Map(s, d);
	}
}";

			var parse = await Parser.GetRootAndModelAsync(code);
			var root = parse.Item1;
			var model = parse.Item2;

			Console.Out.WriteLine(root);

			foreach (var mapInvocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
			{
				var mapMethod = model.GetSymbolInfo(mapInvocation).Symbol as IMethodSymbol;

				if(mapMethod != null &&
					mapMethod.Name == nameof(InlineMapper.Map) && 
					mapMethod.ContainingType.Name == nameof(InlineMapper) &&
					typeof(InlineMapper).AssemblyQualifiedName.Contains(mapMethod.ContainingType.ContainingAssembly.Name))
				{
					var mapInvocationParent = mapInvocation.Parent;
					var sourceArgument = mapInvocation.ArgumentList.Arguments[0];
					var destinationArgument = mapInvocation.ArgumentList.Arguments[1];

					var sourceType = mapMethod.TypeArguments[0];
					var destinationType = mapMethod.TypeArguments[1];
					var destinationProperties = destinationType.GetMembers().OfType<IPropertySymbol>();
					var expressions = new List<SyntaxNode>();
					var comments = new List<SyntaxTrivia>();

					foreach (var sourceProperty in sourceType.GetMembers().OfType<IPropertySymbol>())
					{
						var destinationProperty = destinationProperties.FirstOrDefault(_ => _.Name == sourceProperty.Name);

						if (destinationProperty != null)
						{
							if (destinationProperty.Type.IsAssignableFrom(sourceProperty.Type))
							{
								var expression = $"{destinationArgument.Expression}.{destinationProperty.Name} = {sourceArgument.Expression}.{sourceProperty.Name};";
								expressions.Add(SyntaxFactory.ParseStatement(expression));
							}
							else
							{
								comments.Add(SyntaxFactory.Comment(
									$"// TODO - The source type for the property {sourceProperty.Name}, {sourceProperty.Type.Name}, cannot be assigned to the destination property of type {destinationProperty.Type.Name}"));
							}
						}
						else
						{
							comments.Add(SyntaxFactory.Comment(
								$"// TODO - No destination property could be found for source property {sourceProperty.Name}"));
						}
					}

					if (comments.Count > 0)
					{
						if(expressions.Count > 0)
						{
							expressions[expressions.Count - 1] = 
								expressions[expressions.Count - 1].WithTrailingTrivia(SyntaxFactory.TriviaList(comments));
							var newRoot = root.ReplaceNode(mapInvocationParent, expressions);
							Console.Out.WriteLine(newRoot);
						}
						else
						{
							comments.AddRange(mapInvocationParent.GetTrailingTrivia().ToList());
							var newParent = mapInvocationParent.WithTrailingTrivia(
								SyntaxFactory.TriviaList(comments));

							var newRoot = root.ReplaceNode(mapInvocationParent, newParent);
							Console.Out.WriteLine(newRoot);
						}
					}
					else if(expressions.Count > 0)
					{
						var newRoot = root.ReplaceNode(mapInvocationParent, expressions);
						Console.Out.WriteLine(newRoot);
					}
				}
			}
		}

		private static void ParseMapWithFieldReferences()
		{
			//InlineMapper.Map(fieldSource, fieldDestination);
		}

		private static void ParseMapWithQualifiedFieldReferences()
		{
			//InlineMapper.Map(Program.fieldSource, Program.fieldDestination);
		}

		private static void ParseMapWithNewObject()
		{
			//InlineMapper.Map(source, new Destination());
		}

		private static void ParseMapWithVariablesThatDoNotExist()
		{
			// This is wrong because x and y don't exist, but it seems like nothing is reported in the Syntax Visualizer.
			// InlineMapper.Map(x, y);
			var code = @"
public static class Test
{
	public static void Run()
	{
		InlineMapper.Map(x, y);
	}
}";
			var tree = SyntaxFactory.ParseCompilationUnit(code);
		}
	}
}
