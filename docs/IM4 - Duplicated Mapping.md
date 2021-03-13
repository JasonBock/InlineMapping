# Duplicated Mapping
This will occur if you've already added a mapping attribute in your code that handles the desired mapping. For example, in this code:
```
[assembly: Map(typeof(Source), typeof(Destination))]

[MapTo(typeof(Destination))]
public class Source
{
	public int Identifier { get; set; }
}

public class Destination
{
	public int Id { get; set; }
}
```
This diagnostic will be created as `[Map]` and `[MapTo]` will both map `Source` to `Destination`. It's up to you to decide which attribute to keep.

This is only a warning as having duplicated mapping attributes won't break anything. It's just unnecessary code.