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
    public class Calculator : GlobalInstanceServiceBase, ICalculator
    {
        static int i = 0;
        public Calculator()
        {
            i++;
        }
        public double Add(double a, double b)
        {
            return i;
        }
    }
    [TestFixture]
    public class ServiceTests
    {
       

    }
}
