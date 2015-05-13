using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NUFL.Framework;
using NUFL.Service;
using NUFL.Framework.TestRunner;
using NUFL.Server;

namespace Buaa.NUFL_VSPackage.NUnitTestAdapter
{
    [ExtensionUri(UriString)]
    public class TestExecutor:ITestExecutor
    {
        public const string UriString = "executor://NUFL.NUnitTestExecutor";
        public static Uri Uri = new Uri(UriString);

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        Dictionary<string, TestCase> _cached_test_cases;
        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {


            CacheTestCases(sources);

            using (ITestExectuor executor = GetExecutor(runContext))
            {

                executor.Load(sources);

                executor.RunAllTests(new Listener(frameworkHandle, _cached_test_cases));

                executor.Unload();
            }



        }

        private void CacheTestCases(IEnumerable<string> sources)
        {
            _cached_test_cases = new Dictionary<string, TestCase>();
            var runner_factory = new NUFL.Framework.TestRunner.SimpleRunnerFactory();
            using (var runner = runner_factory.CreateDiscoverer())
            {
                runner.Load(sources);
                var nufl_test_cases = runner.DiscoverTests();
                foreach (var test_case in nufl_test_cases)
                {
                    TestCase vs_test_case = new TestCase(test_case.FullyQualifiedName, TestExecutor.Uri, test_case.AssemblyPath)
                    {
                        DisplayName = test_case.DisplayName,
                        CodeFilePath = test_case.CodeFilePath,
                        LineNumber = test_case.LineNumber,
                    };
                    _cached_test_cases.Add(vs_test_case.FullyQualifiedName, vs_test_case);
                }
                runner.Unload();
            }
        }


        Guid guid = Guid.NewGuid();
        private ITestExectuor GetExecutor(IRunContext runContext)
        {
            string key = runContext.SolutionDirectory;
            if(runContext.IsBeingDebugged)
            {
                var runner_factory = new SimpleRunnerFactory();
                return runner_factory.CreateExecutor();
            } else
            {
                var runner_factory = (NUFL.Framework.TestRunner.ITestRunnerFactory)ServiceManager.Instance.GetService(typeof(NUFL.Framework.TestRunner.ITestRunnerFactory), key);
                return runner_factory.CreateExecutor();
            }

        }


        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            CacheTestCases(tests);

            using (ITestExectuor executor = GetExecutor(runContext))
            {

                IEnumerable<string> sources = GetSources(tests);

                executor.Load(sources);

                executor.RunTests(
                    new List<string>(
                            tests.Select<TestCase, string>(
                                (test) => { return test.FullyQualifiedName; })),
                            new Listener(frameworkHandle, _cached_test_cases));

                executor.Unload();
            }
        }

        private void CacheTestCases(IEnumerable<TestCase> tests)
        {
            _cached_test_cases = new Dictionary<string, TestCase>();
            foreach(var test in tests)
            {
                _cached_test_cases.Add(test.FullyQualifiedName, test);
            }
        }

        private IEnumerable<string> GetSources(IEnumerable<TestCase> tests)
        {
            return new List<string>(tests.Select<TestCase, string>((test) => { return test.Source; }).Distinct<string>());
        }

        class Listener : MarshalByRefObject,ITestResultListener
        {
            IFrameworkHandle _handle;
            Dictionary<string, TestCase> _cache;
            public Listener(IFrameworkHandle handle, Dictionary<string, TestCase> cache)
            {
                _handle = handle;
                _cache = cache;
            }

            public override object InitializeLifetimeService()
            {
                return null;
            }


            public void OnTestResult(NUFL.Framework.TestModel.TestResult result)
            {
                var vs_test_case = _cache[result.FullyQualifiedName];
                var vs_result = new TestResult(vs_test_case)
                {
                    Outcome = mapOutcome(result.Outcome),
                    ErrorMessage = result.ErrorMessage,
                    ErrorStackTrace = result.StackTrace,
                    Duration = result.Duration,
                };
                _handle.RecordResult(vs_result);
            }

            private TestOutcome mapOutcome(NUFL.Framework.TestModel.TestOutcome testOutcome)
            {
                switch(testOutcome)
                {
                    case NUFL.Framework.TestModel.TestOutcome.Passed:
                        return TestOutcome.Passed;
                    case NUFL.Framework.TestModel.TestOutcome.Skipped:
                        return TestOutcome.Skipped;
                    case NUFL.Framework.TestModel.TestOutcome.NotFound:
                        return TestOutcome.NotFound;
                    case NUFL.Framework.TestModel.TestOutcome.None:
                        return TestOutcome.None;
                    case NUFL.Framework.TestModel.TestOutcome.Failed:
                        return TestOutcome.Failed;
                }
                return TestOutcome.None;
            }
        }
    }
}
