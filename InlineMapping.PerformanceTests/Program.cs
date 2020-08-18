using BenchmarkDotNet.Running;
using System;

namespace InlineMapping.PerformanceTests
{
	public static class Program
	{
		public static void Main() =>
			BenchmarkRunner.Run<MappingTests>();
			//Program.RunMappingTests();

		private static void RunMappingTests()
		{
			var tests = new MappingTests();
			tests.MapUsingInline().PrintDestination("Inline");
			Console.Out.WriteLine();
			tests.MapUsingReflection().PrintDestination("Reflection");
			Console.Out.WriteLine();
			tests.MapUsingAutoMapper().PrintDestination("AutoMapper");
		}

		private static void PrintDestination(this Destination destination, string approach)
		{
			Console.Out.WriteLine(approach);
			Console.Out.WriteLine($"{nameof(Destination.Age)} = {destination.Age}");
			Console.Out.WriteLine($"{nameof(Destination.Buffer)} = {destination.Buffer}");
			Console.Out.WriteLine($"{nameof(Destination.Id)} = {destination.Id}");
			Console.Out.WriteLine($"{nameof(Destination.Name)} = {destination.Name}");
			Console.Out.WriteLine($"{nameof(Destination.When)} = {destination.When}");
		}
	}
}