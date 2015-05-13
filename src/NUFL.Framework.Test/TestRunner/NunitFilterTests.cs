using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUFL.Framework.TestRunner;

namespace NUFL.Framework.Test.TestRunner
{
    [TestFixture]
    public class NunitFilterTests
    {
        [Test]
        public void NameFilterTest()
        {
            string[] names = {"method1", "method2", "method3"};
            NameFilter filter = new NameFilter(names);
            Console.WriteLine(filter.Text);
        }
    }
}
