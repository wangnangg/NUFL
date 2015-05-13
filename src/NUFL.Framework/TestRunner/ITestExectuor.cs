using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.TestRunner
{
    public interface ITestExectuor:IDisposable
    {
        void Load(IEnumerable<string> assemblies);

        void Unload();

        void RunTests(IEnumerable<string> full_qualified_names, ITestResultListener listener);

        void RunAllTests(ITestResultListener listener);

    }
}
