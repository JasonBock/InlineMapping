using BenchmarkDotNet.Attributes;
using NUnit.Framework;
using System.Reflection;

namespace InlineMapping.PerformanceTests.Tests;

public static class VerifierTests
{
   [Test]
   public static void Verify()
   {
	  var tests = new MappingTests();

	  var testMethods = tests.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
		  .Where(_ => _.GetCustomAttribute<BenchmarkAttribute>() is not null).ToArray();

	  Assert.That(testMethods.Length, Is.GreaterThan(0));

	  foreach (var testMethod in testMethods)
	  {
		 var result = (Destination)testMethod.Invoke(tests, null)!;

		 Assert.Multiple(() =>
		 {
			Assert.That(result.Id, Is.EqualTo(tests.Source.Id));
			Assert.That(result.Name, Is.EqualTo(tests.Source.Name));
			Assert.That(result.Age, Is.EqualTo(tests.Source.Age));
			Assert.That(result.When, Is.EqualTo(tests.Source.When));
			Assert.That(result.Buffer, Is.EqualTo(tests.Source.Buffer));
		 });
	  }
   }
}