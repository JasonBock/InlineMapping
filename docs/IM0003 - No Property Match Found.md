# No Property Match Found
If a source or destination property could be mapped (i.e. it's public with the right accessibility) but no match was found, this diagnostic is created. In the following code, two diagnostics are created: one for `Identifier` and one for `Id`.
```
[MapTo(typeof(Destination)]
public class Source
{
	public int Identifier { get; set; }
}

public class Destination
{
	public int Id { get; set; }
}
```
In this case, only one is created for the `Name` property on the source type:
```
[MapTo(typeof(Destination)]
public class Source
{
	public int Id { get; set; }
	public string Name { get; set; }
}

public class Destination
{
	public int Id { get; set; }
}
```