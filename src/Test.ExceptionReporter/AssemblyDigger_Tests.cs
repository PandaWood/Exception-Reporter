using System.Linq;
using System.Reflection;
using ExceptionReporting.Report;
using NUnit.Framework;

namespace ExceptionReporting.Tests
{
	public class AssemblyDigger_Tests
	{
		[Test]
		public void Can_Dig_Assembly_Refs_By_Name()
		{
			var digger = new AssemblyDigger(Assembly.Load("ExceptionReporter.NET"));
			var refs = digger.GetAssemblyRefs().ToList();

			Assert.That(refs.Select(r => r.Name), Is.SupersetOf(new [] {"System.Core", "DotNetZip", "SimpleMapi"}));
		}

		[Test]
		public void Can_Memoize_List()
		{
			Assert.That(new AssemblyDigger(Assembly.GetExecutingAssembly()).GetAssemblyRefs(), 
				Is.SameAs(new AssemblyDigger(Assembly.GetExecutingAssembly()).GetAssemblyRefs()));
		}
	}
}