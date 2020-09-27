# No Property Maps Found
If there are no matches between the properties from the source type to the destination type, this error will occur. Following are the scenarios to watch out for.

## Names Do Not Match
This one is straightforward. The property names must be the same.
```
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

## Types Do Not Match
If the names match but the types are not the same, there won't be a match between the two.
```
[MapTo(typeof(Destination))]
public class Source
{
	public int Id { get; set; }
}

public class Destination
{
	public Guid Id { get; set; }
}
```

## No Public Setter For Destination Property Or Public Getter For Source Property
If we can't read the value in the source, or write to it in the destination, we can't map it:
```
[MapTo(typeof(Destination))]
public class Source
{
	public int Id { private get; set; }
}

public class Destination
{
	public Guid Id { get; private set; }
}
```

## Nullable Annotations Do Not Match
This one is a little tricker. If the source property is a non-nullable type, the destination property type can be either nullable or non-nullable:
```
[MapTo(typeof(Destination))]
public class Source
{
	public string Name { get; set; }
}

public class Destination
{
	public string? Name { get; set; }
}
```
However, if the source property is a nullable type, then the destination property type *must* be nullable, because you can't safely assign the contents of a property that might be nullable to a property that is not expecting a null value:
```
[MapTo(typeof(Destination))]
public class Source
{
	public string? Name { get; set; }
}

public class Destination
{
	public string Name { get; set; }
}
```
