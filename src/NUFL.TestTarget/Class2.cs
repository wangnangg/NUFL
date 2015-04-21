using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NUFL.TestTarget
{
    [TestFixture]
    class Class2
    {
        [TestCase(1,1)]
        [TestCase(1, 2)]
        [TestCase(1, 4)]
        public void Test2(int i, int j)
        {

        }
    }
}
