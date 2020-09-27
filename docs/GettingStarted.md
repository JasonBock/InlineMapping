# Getting Started

This is a work in progress - I'll add more information as I make more headway.

## Getting the Code

Right now, you can either clone the repository, or [get it from NuGet](https://www.nuget.org/packages/InlineMapping/). Referencing it is a little tricky. If you reference the project directly, add this to your .csproj file:
```
<ProjectReference Include="..\InlineMapping.Metadata\InlineMapping.Metadata.csproj" />
<ProjectReference Include="..\InlineMapping\InlineMapping.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```
Also, make sure `LangVersion` is set to `preview` in the project that's referencing InlineMapping. Hopefully by the time the source generator feature ships, the details around shipping and referencing generators will be clear.
## How It Works
Once you reference InlineMapping from your project, add the `MapToAttribute` to your source type, specifying what destination types you want to be able to map to:
```
[MapTo(typeof(Destination))]
public class Source
{
	public int Id { get; set; }
}

public class Destination
{
	public int Id { get; set; }
}
```
The generator will automatically create an extension method called `MapToDestination()` within a static class that exists in the same namespace as the source type. It looks something like this:
```
public static partial class SourceMapToExtensions
{
	public static Destination MapToDestination(this Source self) =>
		self is null ? throw new ArgumentNullException(nameof(self)) :
			new Destination
			{
				Id = self.Id,
			};
}
```
If the source type is a value type, then the null check is not performed on the `self` parameter.

To do a mapping, all you need to do is something like this:
```
var source = new Source
{
	Id = 3
};

var destination = source.MapToDestination();
```
If all is well, the `Id` value on `destination` should be 3.
