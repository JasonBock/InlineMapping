# Introduction
New to InlineMapping? In this page, I'll cover the essentials of what InlineMapping can do so you can get up to speed on the API with little effort. I'll go through the attributes you can use to specify which types should be mapped.

Remember that this is just a quickstart. You can always browse the tests in source to see specific examples of a case that may not be covered in detail here.

## How It Works
There are three attributes you can use to specify object mappings: `[Map]`, `[MapFrom]`, and `[MapTo]`. I'll describe how `[MapFrom]` works as the other two attributes are similar. When you add `[MapTo]` to your source type, you specify the destination type you want to be able to map to:
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

`[MapFrom]` is specified on a destination type, and the source type is given to the attribute. `[Map]` is an assembly-level attribute where you specify both the source and destination types - this is useful if you don't own the source code to both types.

## Conclusion
You've now seen the majority of cases that InlineMapping can handle. Remember to peruse the tests within InlineMapping.IntegrationTests in case you get stuck. If you'd like, feel free to submit a PR to update this document to improve its contents. If you run into any issues, please submit an issue. Happy coding!