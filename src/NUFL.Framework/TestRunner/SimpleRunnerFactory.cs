using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.TestRunner
{
    public class SimpleRunnerFactory:ITestRunnerFactory
    {
        public ITestExectuor CreateExecutor()
        {
            return new SimpleTestRunner();
        }

        public ITestDiscoverer CreateDiscoverer()
        {
            return new SimpleTestRunner();
        }
    }
}
