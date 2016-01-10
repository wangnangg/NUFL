using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.TestRunner
{
    public interface INUFLTestEventListener
    {
        void OnTestStart(string fullname);
        void OnTestResult(TestModel.TestResult result);
    }
}
