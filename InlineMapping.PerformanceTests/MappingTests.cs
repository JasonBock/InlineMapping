using AutoMapper;
using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace InlineMapping.PerformanceTests
{
	[MemoryDiagnoser]
	public class MappingTests
	{
		private readonly Source source = new Source
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
		public Destination MapUsingInline() => this.source.MapToDestination();

		[Benchmark]
		public Destination MapUsingReflection()
		{
			var destination = new Destination();
			var sourceProperties = typeof(Source).GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(_ => _.CanRead);

			foreach (var destinationProperty in typeof(Destination).GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(_ => _.CanWrite))
			{
				var sourceProperty = sourceProperties.FirstOrDefault(_ => _.Name == destinationProperty.Name &&
					_.PropertyType == destinationProperty.PropertyType);

				if(sourceProperty is not null)
				{
					destinationProperty.SetValue(destination, sourceProperty.GetValue(this.source));
				}
			}

			return destination;
		}

		[Benchmark]
		public Destination MapUsingAutoMapper() => this.mapper.Map<Destination>(this.source);
	}
}