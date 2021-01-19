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
#pragma warning disable CA1822 // Mark members as static
		public Destination MapUsingInline() => new(); // this.source.MapToDestination();
#pragma warning restore CA1822 // Mark members as static

		[Benchmark]
		public Destination MapUsingReflection()
		{
			Destination destination = new();
			var destinationProperties = typeof(Destination).GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(_ => _.CanWrite);

			foreach (var sourceProperty in typeof(Source).GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(_ => _.CanRead))
			{
				var destinationProperty = destinationProperties.FirstOrDefault(_ => 
					_.Name == sourceProperty.Name &&
					_.PropertyType == sourceProperty.PropertyType);

				if(destinationProperty is not null)
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