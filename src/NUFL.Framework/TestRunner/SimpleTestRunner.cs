using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Engine;
using NUFL.Framework.TestModel;

namespace NUFL.Framework.TestRunner
{
    class SimpleTestRunner:RemoteRunnerBase,ITestDiscoverer,ITestExectuor,ITestEventListener
    {
        TestEngine _engine;
        ITestRunner _runner;
        public SimpleTestRunner()
        {
            _engine = new TestEngine();
        }
        public void Load(IEnumerable<string> assemblies)
        {
            TestPackage package = new TestPackage(new List<string>(assemblies));
            _runner = _engine.GetRunner(package);
            _runner.Load();
        }

        public void Unload()
        {
            try
            {
                _runner.Unload();
                _runner.Dispose();
            }
            catch (Exception e) {}
        }

        public List<TestCase> DiscoverTests()
        {
            List<TestCase> test_cases = TestConverters.ConvertFromNUnitTestCase(_runner.Explore(TestFilter.Empty));
            return test_cases;
        }

        public override void Dispose()
        {
            _engine.Dispose();
            base.Dispose();
        }


        ITestResultListener _listener;

        public void RunTests(IEnumerable<string> full_qualified_names, ITestResultListener listener)
        {
            NameFilter filter = new NameFilter(full_qualified_names);
            RunTests(filter, listener);
        }

        public void RunAllTests(ITestResultListener listener)
        {
            TestFilter filter = TestFilter.Empty;
            RunTests(filter, listener);
        }

        void RunTests(TestFilter filter, ITestResultListener listener)
        {
            _listener = listener;
            _runner.Run(this, filter);
        }


        public void OnTestEvent(string report)
        {
            try
            {
                TestResult result = TestConverters.ConvertFromNUnitTestResult(report);
                if (_listener != null)
                {
                    _listener.OnTestResult(result);
                }
            }
            catch (Exception e)
            {
               // System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
    }
}
