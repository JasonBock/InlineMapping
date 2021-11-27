using InlineMapping.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace InlineMapping.Tests;

public static class DuplicationTests
{
   [Test]
   public static async Task DuplicateMapFromAndMapToAsync()
   {
	  var code =
@"using InlineMapping;

[MapFrom(typeof(Source))]
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

	  var diagnostic = new DiagnosticResult(DuplicatedAttributeDiagnostic.Id, DiagnosticSeverity.Warning)
		  .WithSpan(9, 2, 9, 28);
	  await TestAssistants.RunAsync(code,
		  new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
		  new[] { diagnostic }).ConfigureAwait(false);
   }

   [Test]
   public static async Task DuplicateMapFromAndMapAsync()
   {
	  var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

[MapFrom(typeof(Source))]
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

	  var diagnostic = new DiagnosticResult(DuplicatedAttributeDiagnostic.Id, DiagnosticSeverity.Warning)
		  .WithSpan(5, 2, 5, 25);
	  await TestAssistants.RunAsync(code,
		  new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
		  new[] { diagnostic }).ConfigureAwait(false);
   }

   [Test]
   public static async Task DuplicateMapToAndMapAsync()
   {
	  var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

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

	  var diagnostic = new DiagnosticResult(DuplicatedAttributeDiagnostic.Id, DiagnosticSeverity.Warning)
		  .WithSpan(10, 2, 10, 28);
	  await TestAssistants.RunAsync(code,
		  new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
		  new[] { diagnostic }).ConfigureAwait(false);
   }
}