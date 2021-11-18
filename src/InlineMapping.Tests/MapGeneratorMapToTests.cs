using InlineMapping.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace InlineMapping.Tests;

public static class MapGeneratorMapToTests
{
	[Test]
	public static async Task GenerateWithClassesAsync()
	{
		var code =
 @"using InlineMapping;

public class Destination 
{ 
	public string Id { get; set; }
}

[MapTo(typeof(Destination))]
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
	public static async Task GenerateWithStructsAsync()
	{
		var code =
 @"using InlineMapping;

public struct Destination 
{ 
	public string Id { get; set; }
}

[MapTo(typeof(Destination))]
public struct Source 
{ 
	public string Id { get; set; }
}";

		var generatedCode =
 @"#nullable enable

public static partial class SourceMapToExtensions
{
	public static Destination MapToDestination(this Source self) =>
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
	public static async Task GenerateWithRecordsAsync()
	{
		var code =
 @"using InlineMapping;

public class Destination 
{ 
	public string Id { get; set; }
}

[MapTo(typeof(Destination))]
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
	public static async Task GenerateWhenSourceIsInNamespaceAndDestinationIsNotInNamespaceAsync()
	{
		var code =
 @"using InlineMapping;

public class Destination 
{ 
	public string Id { get; set; }
}

namespace SourceNamespace
{
	[MapTo(typeof(Destination))]
	public class Source 
	{ 
		public string Id { get; set; }
	}
}";

		var generatedCode =
 @"using System;

#nullable enable

namespace SourceNamespace
{
	public static partial class SourceMapToExtensions
	{
		public static Destination MapToDestination(this Source self) =>
			self is null ? throw new ArgumentNullException(nameof(self)) :
				new Destination
				{
					Id = self.Id,
				};
	}
}
";

		await TestAssistants.RunAsync(code,
			new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenSourceIsNotInNamespaceAndDestinationIsInNamespaceAsync()
	{
		var code =
 @"using InlineMapping;

namespace DestinationNamespace
{
	public class Destination 
	{ 
		public string Id { get; set; }
	}
}

[MapTo(typeof(DestinationNamespace.Destination))]
public class Source 
{ 
	public string Id { get; set; }
}";

		var generatedCode =
 @"using DestinationNamespace;
using System;

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
	public static async Task GenerateWhenDestinationIsInSourceNamespaceAsync()
	{
		var code =
 @"using InlineMapping;

namespace BaseNamespace
{
	public class Destination 
	{ 
		public string Id { get; set; }
	}
}

namespace BaseNamespace.SubNamespace
{
	[MapTo(typeof(Destination))]
	public class Source 
	{ 
		public string Id { get; set; }
	}
}";

		var generatedCode =
 @"using System;

#nullable enable

namespace BaseNamespace.SubNamespace
{
	public static partial class SourceMapToExtensions
	{
		public static Destination MapToDestination(this Source self) =>
			self is null ? throw new ArgumentNullException(nameof(self)) :
				new Destination
				{
					Id = self.Id,
				};
	}
}
";

		await TestAssistants.RunAsync(code,
			new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenDestinationIsNotInSourceNamespaceAsync()
	{
		var code =
 @"using InlineMapping;

namespace DestinationNamespace
{
	public class Destination 
	{ 
		public string Id { get; set; }
	}
}

namespace SourceNamespace
{
	[MapTo(typeof(DestinationNamespace.Destination))]
	public class Source 
	{ 
		public string Id { get; set; }
	}
}";

		var generatedCode =
 @"using DestinationNamespace;
using System;

#nullable enable

namespace SourceNamespace
{
	public static partial class SourceMapToExtensions
	{
		public static Destination MapToDestination(this Source self) =>
			self is null ? throw new ArgumentNullException(nameof(self)) :
				new Destination
				{
					Id = self.Id,
				};
	}
}
";

		await TestAssistants.RunAsync(code,
			new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenNoPropertiesExistAsync()
	{
		var code =
 @"using InlineMapping;

public class Destination { }

[MapTo(typeof(Destination))]
public class Source { }";

		var diagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(5, 1, 6, 24);
		await TestAssistants.RunAsync(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { diagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateIncrementalWhenNoPropertiesExistAsync()
	{
		var code =
 @"using InlineMapping;

public class Destination { }

[MapTo(typeof(Destination))]
public class Source { }";

		var diagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(5, 1, 6, 24);
		await TestAssistants.RunAsync(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { diagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenSourcePropertyIsNotPublicAsync()
	{
		var code =
 @"using InlineMapping;

public class Destination 
{ 
	public string Id { get; set; }
}

[MapTo(typeof(Destination))]
public class Source 
{ 
	private string Id { get; set; }
}";

		var mapDiagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(8, 1, 12, 2);
		var matchDiagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
		await TestAssistants.RunAsync(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { mapDiagnostic, matchDiagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenDestinationPropertyIsNotPublicAsync()
	{
		var code =
 @"using InlineMapping;

public class Destination 
{ 
	private string Id { get; set; }
}

[MapTo(typeof(Destination))]
public class Source 
{ 
	public string Id { get; set; }
}";

		var mapDiagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(8, 1, 12, 2);
		var matchDiagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
		await TestAssistants.RunAsync(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { mapDiagnostic, matchDiagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenSourceGetterIsNotPublicAsync()
	{
		var code =
 @"using InlineMapping;

public class Destination 
{ 
	public string Id { get; set; }
}

[MapTo(typeof(Destination))]
public class Source 
{ 
	public string Id { private get; set; }
}";

		var mapDiagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(8, 1, 12, 2);
		var matchDiagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
		await TestAssistants.RunAsync(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { mapDiagnostic, matchDiagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenDestinationSetterIsNotPublicAsync()
	{
		var code =
 @"using InlineMapping;

public class Destination 
{ 
	public string Id { get; private set; }
}

[MapTo(typeof(Destination))]
public class Source 
{ 
	public string Id { get; set; }
}";

		var mapDiagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(8, 1, 12, 2);
		var matchDiagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
		await TestAssistants.RunAsync(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { mapDiagnostic, matchDiagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenDestinationHasNoAccessibleConstructorsAsync()
	{
		var code =
 @"using InlineMapping;

public class Destination 
{
	private Destination() { }

	public string Id { get; set; }
}

[MapTo(typeof(Destination))]
public class Source 
{ 
	public string Id { get; set; }
}";

		var diagnostic = new DiagnosticResult(NoAccessibleConstructorsDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(10, 1, 14, 2);
		await TestAssistants.RunAsync(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { diagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenSourceDoesNotMapAllPropertiesAsync()
	{
		var code =
 @"using InlineMapping;

public class Destination 
{ 
	public string Id { get; set; }
}

[MapTo(typeof(Destination))]
public class Source 
{ 
	public string Id { get; set; }
	public string Name { get; set; }
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

		var diagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
		await TestAssistants.RunAsync(code,
			new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
			new[] { diagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenDestinationDoesNotMapAllPropertiesAsync()
	{
		var code =
 @"using InlineMapping;

public class Destination 
{ 
	public string Id { get; set; }
	public string Name { get; set; }
}

[MapTo(typeof(Destination))]
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

		var diagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
		await TestAssistants.RunAsync(code,
			new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
			new[] { diagnostic }).ConfigureAwait(false);
	}

	[Test]
	public static async Task GenerateWhenPropertyTypesDoNotMatchAsync()
	{
		var code =
 @"using InlineMapping;

public class Destination 
{ 
	public string Id { get; set; }
}

[MapTo(typeof(Destination))]
public class Source 
{ 
	public int Id { get; set; }
}";

		var mapDiagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(8, 1, 12, 2);
		var matchDiagnostic1 = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
		var matchDiagnostic2 = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
		await TestAssistants.RunAsync(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { mapDiagnostic, matchDiagnostic1, matchDiagnostic2 }).ConfigureAwait(false);
	}
}