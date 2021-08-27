using InlineMapping.Descriptors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InlineMapping.Tests
{
	public static class MapGeneratorMapTests
	{
		[Test]
		public static async Task GenerateWithClassesAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

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
		public static async Task GenerateWithStructsAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public struct Destination 
{ 
	public string Id { get; set; }
}

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
				Enumerable.Empty<DiagnosticResult>());
		}

		[Test]
		public static async Task GenerateWithRecordsAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public record Destination 
{ 
	public string Id { get; init; }
}

public record Source 
{ 
	public string Id { get; init; }
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
		public static async Task GenerateWhenSourceIsInNamespaceAndDestinationIsNotInNamespaceAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(SourceNamespace.Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; set; }
}

namespace SourceNamespace
{
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
				Enumerable.Empty<DiagnosticResult>());
		}

		[Test]
		public static async Task GenerateWhenSourceIsNotInNamespaceAndDestinationIsInNamespaceAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(DestinationNamespace.Destination))]

namespace DestinationNamespace
{
	public class Destination 
	{ 
		public string Id { get; set; }
	}
}

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
				Enumerable.Empty<DiagnosticResult>());
		}

		[Test]
		public static async Task GenerateWhenDestinationIsInSourceNamespaceAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(BaseNamespace.SubNamespace.Source), typeof(BaseNamespace.Destination))]

namespace BaseNamespace
{
	public class Destination 
	{ 
		public string Id { get; set; }
	}
}

namespace BaseNamespace.SubNamespace
{
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
				Enumerable.Empty<DiagnosticResult>());
		}

		[Test]
		public static async Task GenerateWhenDestinationIsNotInSourceNamespaceAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(SourceNamespace.Source), typeof(DestinationNamespace.Destination))]

namespace DestinationNamespace
{
	public class Destination 
	{ 
		public string Id { get; set; }
	}
}

namespace SourceNamespace
{
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
				Enumerable.Empty<DiagnosticResult>());
		}

		[Test]
		public static async Task GenerateWhenNoPropertiesExistAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination { }

public class Source { }";

			var diagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
				.WithSpan(3, 12, 3, 52);
			await TestAssistants.RunAsync(code,
				Enumerable.Empty<(Type, string, string)>(),
				new[] { diagnostic });
		}

		[Test]
		public static async Task GenerateWhenSourcePropertyIsNotPublicAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; set; }
}

public class Source 
{ 
	private string Id { get; set; }
}";

			var mapDiagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
				.WithSpan(3, 12, 3, 52);
			var matchDiagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
			await TestAssistants.RunAsync(code,
				Enumerable.Empty<(Type, string, string)>(),
				new[] { mapDiagnostic, matchDiagnostic });
		}

		[Test]
		public static async Task GenerateWhenDestinationPropertyIsNotPublicAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	private string Id { get; set; }
}

public class Source 
{ 
	public string Id { get; set; }
}";

			var mapDiagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
				.WithSpan(3, 12, 3, 52);
			var matchDiagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
			await TestAssistants.RunAsync(code,
				Enumerable.Empty<(Type, string, string)>(),
				new[] { mapDiagnostic, matchDiagnostic });
		}

		[Test]
		public static async Task GenerateWhenSourceGetterIsNotPublicAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; set; }
}

public class Source 
{ 
	public string Id { private get; set; }
}";

			var mapDiagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
				.WithSpan(3, 12, 3, 52);
			var matchDiagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
			await TestAssistants.RunAsync(code,
				Enumerable.Empty<(Type, string, string)>(),
				new[] { mapDiagnostic, matchDiagnostic });
		}

		[Test]
		public static async Task GenerateWhenDestinationSetterIsNotPublicAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; private set; }
}

public class Source 
{ 
	public string Id { get; set; }
}";

			var mapDiagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
				.WithSpan(3, 12, 3, 52);
			var matchDiagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
			await TestAssistants.RunAsync(code,
				Enumerable.Empty<(Type, string, string)>(),
				new[] { mapDiagnostic, matchDiagnostic });
		}

		[Test]
		public static async Task GenerateWhenDestinationHasNoAccessibleConstructorsAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{
	private Destination() { }

	public string Id { get; set; }
}

public class Source 
{ 
	public string Id { get; set; }
}";

			var diagnostic = new DiagnosticResult(NoAccessibleConstructorsDiagnostic.Id, DiagnosticSeverity.Error)
				.WithSpan(3, 12, 3, 52);
			await TestAssistants.RunAsync(code,
				Enumerable.Empty<(Type, string, string)>(),
				new[] { diagnostic });
		}

		[Test]
		public static async Task GenerateWhenSourceDoesNotMapAllPropertiesAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; set; }
}

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
				new[] { diagnostic });
		}

		[Test]
		public static async Task GenerateWhenDestinationDoesNotMapAllPropertiesAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; set; }
	public string Name { get; set; }
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

			var diagnostic = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
			await TestAssistants.RunAsync(code,
				new[] { (typeof(MapGenerator), "Source_To_Destination_Map.g.cs", generatedCode) },
				new[] { diagnostic });
		}

		[Test]
		public static async Task GenerateWhenPropertyTypesDoNotMatchAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(Source), typeof(Destination))]

public class Destination 
{ 
	public string Id { get; set; }
}

public class Source 
{ 
	public int Id { get; set; }
}";

			var mapDiagnostic = new DiagnosticResult(NoPropertyMapsFoundDiagnostic.Id, DiagnosticSeverity.Error)
				.WithSpan(3, 12, 3, 52);
			var matchDiagnostic1 = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
			var matchDiagnostic2 = new DiagnosticResult(NoMatchDiagnostic.Id, DiagnosticSeverity.Info);
			await TestAssistants.RunAsync(code,
				Enumerable.Empty<(Type, string, string)>(),
				new[] { mapDiagnostic, matchDiagnostic1, matchDiagnostic2 });
		}

		[Test]
		public static async Task GenerateWhenDestinationNamespaceIsSelectedAsync()
		{
			var code =
@"using InlineMapping;

[assembly: Map(typeof(SourceNamespace.Source), typeof(DestinationNamespace.Destination), ContainingNamespaceKind.Destination)]

namespace DestinationNamespace
{
	public class Destination 
	{ 
		public string Id { get; set; }
	}
}

namespace SourceNamespace
{
	public class Source 
	{ 
		public string Id { get; set; }
	}
}";

			var generatedCode =
@"using SourceNamespace;
using System;

#nullable enable

namespace DestinationNamespace
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
				Enumerable.Empty<DiagnosticResult>());
		}
	}
}