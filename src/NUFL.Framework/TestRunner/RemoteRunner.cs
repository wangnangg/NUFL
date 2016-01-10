using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Service;
using NUFL.Framework.TestModel;
using System.Runtime.Remoting.Lifetime;
using System.Diagnostics;
namespace NUFL.Framework.TestRunner
{
    public class RemoteRunner : GlobalProcessService, INUFLTestRunner
    {
        INUFLTestRunner _inner_runner;
        public RemoteRunner(INUFLTestRunner inner_runner, int owner_pid)
            : base(owner_pid)
        {
            _inner_runner = inner_runner;
        }

        public void Load(IEnumerable<string> assemblies)
        {
            _inner_runner.Load(assemblies);
        }

        public void RunTests(IEnumerable<string> full_qualified_names, INUFLTestEventListener listener)
        {
            _inner_runner.RunTests(full_qualified_names, listener);
        }

        public void StopRun()
        {
            _inner_runner.StopRun();
        }

        public List<TestCase> DiscoverTests()
        {
            return _inner_runner.DiscoverTests();
        }

        public override void Dispose()
        {
            _inner_runner.Dispose();
            base.Dispose();
        }

    

       
    }
}
