# Missing No-Argument Constructor On Destination Type
If the destination type does not have a public, no-argument constructor, this error is generated. The generated mapping code has to be able to create an instance of the destination type. The following code is an example of what would cause this error to occur.
```
[MapTo(typeof(Destination)]
public class Source
{
	public int Id { get; set; }
}

public class Destination
{
	private Destination() { }
	
	public int Id { get; set; }
}
```