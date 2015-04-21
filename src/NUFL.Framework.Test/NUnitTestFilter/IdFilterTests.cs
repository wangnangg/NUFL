using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUFL.Framework.NUnitTestFilter;

namespace NUFL.Framework.Test.NUnitTestFilter
{
    [TestFixture]
    class IdFilterTests
    {
        [Test]
        public void IdFilterOneId()
        {
            IdFilter filter = new IdFilter();
            filter.Add(1);
            System.Diagnostics.Debug.WriteLine(filter.Text);
           
        }
        [Test]
        public void IdFilterThreeId()
        {
            IdFilter filter = new IdFilter();
            filter.Add(1);
            filter.Add(2);
            filter.Add(3);
            System.Diagnostics.Debug.WriteLine(filter.Text);
        }
        [Test]
        public void IdFilterThreeId2()
        {
            IdFilter filter = new IdFilter(new int[]{1,2,3});
            System.Diagnostics.Debug.WriteLine(filter.Text);

        }
    }
}
