using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.TestRunner
{
    public interface ITestResultListener
    {
        void OnTestResult(TestModel.TestResult result);
    }
}
