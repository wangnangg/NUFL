using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUFL.Service;
namespace NUFL.Service.Test
{
    interface ICalculator
    {
        double Add(double a, double b);
    }
    public class Calculator : GlobalServiceBase, ICalculator
    {
        static int i = 0;
        public Calculator()
        {
            i++;
        }
        public double Add(double a, double b)
        {
            return a + b + i;
        }
    }
    [TestFixture]
    public class Class1
    {
        [Test]
        public void ServiceTest()
        {
            ServiceManager mgr = new ServiceManager(3247);
            mgr.ProvideGlobalService(typeof(ICalculator), new Calculator());  
            mgr.Start();

            ServiceManager mgr2 = new ServiceManager(3248);
            mgr2.RequireService(typeof(ICalculator), 3247);
            mgr2.Start();
            ICalculator cal = mgr2.GetService(typeof(ICalculator)) as ICalculator;
            Console.WriteLine(cal.Add(1, 2));
            cal = mgr2.GetService(typeof(ICalculator)) as ICalculator;
            Console.WriteLine(cal.Add(1, 2));
            cal = mgr2.GetService(typeof(ICalculator)) as ICalculator;
            Console.WriteLine(cal.Add(1, 2));
        }
    }
}
