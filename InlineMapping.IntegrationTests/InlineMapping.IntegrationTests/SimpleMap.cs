namespace InlineMapping.IntegrationTests
{
	public class SimpleMap
	{
		public class BaseClass { }
		public class SubClass : BaseClass { }

		public class Source
		{
			public int A { get; set; }
			public int B { get; set; }
			public SubClass F { get; set; }
			public BaseClass G { get; set; }
		}

		public class Destination
		{
			public int A { get; set; }
			public int B { get; set; }
			public BaseClass F { get; set; }
			public SubClass G { get; set; }
		}

		public void Do()
		{
			var s = new Source();
			var d = new Destination();
			InlineMapper.Map(s, d);
		}
	}
}
