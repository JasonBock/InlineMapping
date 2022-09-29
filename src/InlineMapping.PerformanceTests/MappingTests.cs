using AutoMapper;
using BenchmarkDotNet.Attributes;
using Csla.Data;
using Mapster;
using ServiceStack;
using System.Reflection;

namespace InlineMapping.PerformanceTests;

[MemoryDiagnoser]
public class MappingTests
{
	private readonly Source source = new()
	{
		Age = 22,
		Buffer = Guid.NewGuid().ToByteArray(),
		Id = Guid.NewGuid(),
		Name = "Stephanie",
		When = DateTime.Now.AddDays(-2)
	};

	private readonly IMapper mapper =
		new MapperConfiguration(_ => _.CreateMap<Source, Destination>()).CreateMapper();

	[Benchmark(Baseline = true)]
	public Destination InlineMapping() => this.source.MapToDestination();

	[Benchmark]
	public Destination Reflection()
	{
		var destination = new Destination();
		var destinationProperties = typeof(Destination).GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Where(_ => _.CanWrite);

		foreach (var sourceProperty in typeof(Source).GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Where(_ => _.CanRead))
		{
			var destinationProperty = destinationProperties.FirstOrDefault(_ =>
				_.Name == sourceProperty.Name &&
				_.PropertyType == sourceProperty.PropertyType);

			if (destinationProperty is not null)
			{
				destinationProperty.SetValue(destination, sourceProperty.GetValue(this.source));
			}
		}

		return destination;
	}

	[Benchmark]
	public Destination AutoMapper() => this.mapper.Map<Destination>(this.source);

	[Benchmark]
	public Destination Mapster() => this.source.Adapt<Destination>();

	[Benchmark]
	public Destination MappingGenerator() => MappingTests.Map(this.source);

	[Benchmark]
	public Destination ServiceStack() => this.source.ConvertTo<Destination>();

	[Benchmark]
	public Destination Csla()
	{
		var destination = new Destination();
		DataMapper.Map(this.source, destination);
		return destination;
	}

	private static Destination Map(Source source) => new()
	{
		Age = source.Age,
		Buffer = source.Buffer,
		Id = source.Id,
		Name = source.Name,
		When = source.When
	};

	public Source Source => this.source;
}