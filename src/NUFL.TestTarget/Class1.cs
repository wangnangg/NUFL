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
                Method2();
                for (int i = 0; i < 1000; i++ )
                    a = -a;
                Assert.AreEqual(0, 1);
            } else
            {
                Method2();
                for (int i = 0; i < 1000; i++)
                a = a * 2;
                Assert.AreEqual(0, 0);
            }
        }
        [Test]
        public void Method2()
        {
            int a = -1;
            if (a > 0)
            {
                for (int i = 0; i < 1000; i++)
                a = -a;
                Assert.AreEqual(0, 1);
            }
            else
            {
                for (int i = 0; i < 1000; i++)
                a = a * 2;
                Assert.AreEqual(0, 0);
            }
        }
    }
}
