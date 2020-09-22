using BenchmarkDotNet.Running;
using InlineMapping.PerformanceTests;
using System;

BenchmarkRunner.Run<MappingTests>();
//Program.RunMappingTests();

//static void RunMappingTests()
//{
//	var tests = new MappingTests();
//	tests.MapUsingInline().PrintDestination("Inline");
//	Console.Out.WriteLine();
//	tests.MapUsingReflection().PrintDestination("Reflection");
//	Console.Out.WriteLine();
//	tests.MapUsingAutoMapper().PrintDestination("AutoMapper");
//}

//static class DestinationExtensions
//{
//	public static void PrintDestination(this Destination destination, string approach)
//	{
//		Console.Out.WriteLine(approach);
//		Console.Out.WriteLine($"{nameof(Destination.Age)} = {destination.Age}");
//		Console.Out.WriteLine($"{nameof(Destination.Buffer)} = {destination.Buffer}");
//		Console.Out.WriteLine($"{nameof(Destination.Id)} = {destination.Id}");
//		Console.Out.WriteLine($"{nameof(Destination.Name)} = {destination.Name}");
//		Console.Out.WriteLine($"{nameof(Destination.When)} = {destination.When}");
//	}
//}