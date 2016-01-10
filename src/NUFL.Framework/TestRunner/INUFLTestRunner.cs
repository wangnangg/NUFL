using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.TestModel;

namespace NUFL.Framework.TestRunner
{
    public interface INUFLTestRunner:IDisposable
    {
        void Load(IEnumerable<string> assemblies);

        void RunTests(IEnumerable<string> full_qualified_names, INUFLTestEventListener listener);

        void StopRun();

        List<TestCase> DiscoverTests();
    }
}
