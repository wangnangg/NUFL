using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
namespace NUFL.TestTarget
{
    
    [TestFixture]
    public class Class1
    {
        [Test]
        public void Method1()
        {
            int a = 1;
            if( a > 0)
            {
                a = -a;
            } else
            {
                a = a * 2;
            }
        }
    }
}
