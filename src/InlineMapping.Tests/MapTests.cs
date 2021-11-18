using InlineMapping.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace InlineMapping.Tests;

public static class MapTests
{
	[Test]
	public static async Task MapWhenDuplicateMappingsExistAsync()
	{
		var code =
 @"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

[MapTo(typeof(Destination))]
public class Source
{
	public int Id { get; set; }
}

public class Destination
{
	public int Id { get; set; }
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
		var duplicateDiagnostic = new DiagnosticResult(DuplicatedAttributeDiagnostic.Id, DiagnosticSeverity.Warning)
			.WithSpan(5, 1, 9, 2).WithArguments("Map(typeof(Source), typeof(Destination))");
		await TestAssistants.RunAsync(code,
			new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
			new[] { duplicateDiagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task MapWhenSourcePropertyDoesNotMapAsync()
	{
		var code =
 @"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Source
{
	public string Name { get; set; }
	public int Id { get; set; }
}

public class Destination
{
	public int Id { get; set; }
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
		var noMatchDiagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
		await TestAssistants.RunAsync(code,
			new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
			new[] { noMatchDiagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task MapWhenDestinationPropertyDoesNotMapAsync()
	{
		var code =
 @"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Source
{
	public int Id { get; set; }
}

public class Destination
{
	public string Name { get; set; }
	public int Id { get; set; }
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
		var noMatchDiagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
		await TestAssistants.RunAsync(code,
			new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
			new[] { noMatchDiagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task MapWhenNoMapsExistAsync()
	{
		var code =
 @"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Source
{
	public int Id { get; set; }
}

public class Destination
{
	public string Name { get; set; }
}";

		var noMatchSourceDiagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
		var noMatchDestinationDiagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
		var noPropertyMatchDiagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(3, 12, 3, 52);
		await TestAssistants.RunAsync(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { noMatchSourceDiagnostic, noMatchDestinationDiagnostic, noPropertyMatchDiagnostic }).ConfigureAwait(false);
	}

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
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
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
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
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
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
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
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
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
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
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
			new[] { mapDiagnostic, matchDiagnostic1, matchDiagnostic2 }).ConfigureAwait(false);
	}

	[Test]
	public static async Task MapWithNoAccessibleConstructorsAsync()
	{
		var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Source
{
	public int Id { get; set; }
}

public class Destination
{
	private Destination() { }

	public int Id { get; set; }
}";

		var constructorDiagnostic = new DiagnosticResult(NoAccessibleConstructorsDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(3, 12, 3, 52);
		await TestAssistants.RunAsync(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { constructorDiagnostic }).ConfigureAwait(false);
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
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}
}