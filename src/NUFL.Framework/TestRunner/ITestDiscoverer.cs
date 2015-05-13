using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.TestModel;

namespace NUFL.Framework.TestRunner
{
    public interface ITestDiscoverer : IDisposable
    {
        void Load(IEnumerable<string> assemblies);

        void Unload();

        List<TestCase> DiscoverTests();
    }
}
