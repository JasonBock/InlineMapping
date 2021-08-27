using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System.Collections.Immutable;
using System;
using System.Linq;
using InlineMapping.Descriptors;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;

namespace InlineMapping.Tests
{
	public static class MapTests
	{
		[Test]
		public static async Task MapToSelfAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Source))]

public class Source 
{ 
	public string Id { get; set; }
}";

			var generatedCode =
@"using System;

#nullable enable

public static partial class SourceMapToExtensions
{
	public static Source MapToSource(this Source self) =>
		self is null ? throw new ArgumentNullException(nameof(self)) :
			new Source
			{
				Id = self.Id,
			};
}
";

			await TestAssistants.RunAsync(code,
				new[] { (typeof(MapGenerator), "Source_To_Source_Map.g.cs", generatedCode) },
				Enumerable.Empty<DiagnosticResult>());
		}

		[Test]
		public static async Task MapWithSourcePropertyHavingInternalGetterAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Source 
{ 
	internal string Id { get; set; }
}

public class Destination
{ 
	public string Id { get; set; }
}";

			var generatedCode =
@"using System;

#nullable enable

public static partial class SourceMapToExtensions
{
	public static Destination MapToDestination(this Source self) =>
		self is null ? throw new ArgumentNullException(nameof(self)) :
			new Destination
			{
				Id = self.Id,
			};
}
";

			await TestAssistants.RunAsync(code,
				new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
				Enumerable.Empty<DiagnosticResult>());
		}

		[Test]
		public static async Task MapWithDestinationPropertyHavingInternalSetterAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Source 
{ 
	public string Id { get; set; }
}

public class Destination
{ 
	internal string Id { get; set; }
}";

			var generatedCode =
@"using System;

#nullable enable

public static partial class SourceMapToExtensions
{
	public static Destination MapToDestination(this Source self) =>
		self is null ? throw new ArgumentNullException(nameof(self)) :
			new Destination
			{
				Id = self.Id,
			};
}
";

			await TestAssistants.RunAsync(code,
				new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
				Enumerable.Empty<DiagnosticResult>());
		}

		[Test]
		public static async Task MapWithMatchingPropertyTypeKindAsExactAndTypesAreExactAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination), matchingPropertyTypeKind: MatchingPropertyTypeKind.Exact)]

public class Destination 
{ 
	public string Id { get; set; }
}

public class Source 
{ 
	public string Id { get; set; }
}";

			var generatedCode =
@"using System;

#nullable enable

public static partial class SourceMapToExtensions
{
	public static Destination MapToDestination(this Source self) =>
		self is null ? throw new ArgumentNullException(nameof(self)) :
			new Destination
			{
				Id = self.Id,
			};
}
";

			await TestAssistants.RunAsync(code,
				new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
				Enumerable.Empty<DiagnosticResult>());
		}

		[Test]
		public static async Task MapWithMatchingPropertyTypeKindAsImplicitAndTypesAreExactAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination), matchingPropertyTypeKind: MatchingPropertyTypeKind.Implicit)]

public class Destination 
{ 
	public string Id { get; set; }
}

public class Source 
{ 
	public string Id { get; set; }
}";

			var generatedCode =
@"using System;

#nullable enable

public static partial class SourceMapToExtensions
{
	public static Destination MapToDestination(this Source self) =>
		self is null ? throw new ArgumentNullException(nameof(self)) :
			new Destination
			{
				Id = self.Id,
			};
}
";

			await TestAssistants.RunAsync(code,
				new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
				Enumerable.Empty<DiagnosticResult>());
		}

		[Test]
		public static async Task MapWithMatchingPropertyTypeKindAsExactAndTypesAreImplicitAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination), matchingPropertyTypeKind: MatchingPropertyTypeKind.Exact)]

public class A { }

public class B : A { }

public class Destination 
{ 
	public A Id { get; set; }
}

public class Source 
{ 
	public B Id { get; set; }
}";

			var mapDiagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
				.WithSpan(3, 12, 3, 110);
			var matchDiagnostic1 = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
			var matchDiagnostic2 = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
			await TestAssistants.RunAsync(code,
				Enumerable.Empty<(Type, string, string)>(),
				new[] { mapDiagnostic, matchDiagnostic1, matchDiagnostic2 });
		}

		[Test]
		public static async Task MapWithMatchingPropertyTypeKindAsImplicitAndTypesAreImplicitAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination), matchingPropertyTypeKind: MatchingPropertyTypeKind.Implicit)]

public class A { }

public class B : A { }

public class Destination 
{ 
	public A Id { get; set; }
}

public class Source 
{ 
	public B Id { get; set; }
}";

			var generatedCode =
@"using System;

#nullable enable

public static partial class SourceMapToExtensions
{
	public static Destination MapToDestination(this Source self) =>
		self is null ? throw new ArgumentNullException(nameof(self)) :
			new Destination
			{
				Id = self.Id,
			};
}
";

			await TestAssistants.RunAsync(code,
				new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
				Enumerable.Empty<DiagnosticResult>());
		}
	}
}