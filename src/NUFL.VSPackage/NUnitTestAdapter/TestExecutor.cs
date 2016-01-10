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
using System.Diagnostics;
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
        INUFLTestRunner _current_executor;
        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {

            CacheTestCases(sources);

            using (INUFLTestRunner executor = GetExecutor(runContext, frameworkHandle))
            {
                if(executor == null)
                {
                    frameworkHandle.SendMessage(Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.TestMessageLevel.Error, "NUFL: Cannot locate solution directory.");
                    return;
                }
                _current_executor = executor;
                //var listener = new Listener(frameworkHandle, _cached_test_cases);

                executor.Load(sources);

                executor.RunTests(null,new Listener(frameworkHandle, _cached_test_cases));

                _current_executor = null;

            }



        }

        private void CacheTestCases(IEnumerable<string> sources)
        {
            _cached_test_cases = new Dictionary<string, TestCase>();
            var runner_factory = new NUFL.Framework.TestRunner.RunnerFactory();
            runner_factory.IsX64 = Environment.Is64BitProcess;
            runner_factory.OwnerPid = Process.GetCurrentProcess().Id;
            using (var runner = runner_factory.GetProcessRunner(null))
            {
                runner.Load(sources);
                var nufl_test_cases = runner.DiscoverTests();
                foreach (var test_case in nufl_test_cases)
                {
                    var vs_test_case = Converter.ConvertFromNUFLTestCase(test_case);
                    _cached_test_cases[vs_test_case.FullyQualifiedName] = vs_test_case;
                }
            }
        }


        Guid guid = Guid.NewGuid();
        private INUFLTestRunner GetExecutor(IRunContext runContext, IFrameworkHandle framework_handle)
        {
            string key = runContext.SolutionDirectory;
            int pid = Process.GetCurrentProcess().Id;
            if(key == "")
            {
                return null;
            }
            if(runContext.IsBeingDebugged)
            {
                var runner_factory = new NUFL.Framework.TestRunner.RunnerFactory();
                runner_factory.IsX64 = Environment.Is64BitProcess;
                runner_factory.OwnerPid = Process.GetCurrentProcess().Id;
                return runner_factory.GetProcessRunner((filepath, argument) =>
                    {
                        framework_handle.LaunchProcessWithDebuggerAttached(filepath, System.Environment.CurrentDirectory, argument, null);
                    });
            } else
            {
                var runner_factory = RemoteRunnerFactory.GetRemoteRunnerFactory(key);
                runner_factory.IsX64 = Environment.Is64BitProcess;
                runner_factory.OwnerPid = Process.GetCurrentProcess().Id;
                return runner_factory.GetRunner();
            }

        }


        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            CacheTestCases(tests);

            using (INUFLTestRunner executor = GetExecutor(runContext, frameworkHandle))
            {

                IEnumerable<string> sources = GetSources(tests);

                executor.Load(sources);

                executor.RunTests(
                    new List<string>(
                            tests.Select<TestCase, string>(
                                (test) => { return test.FullyQualifiedName; })),
                            new Listener(frameworkHandle, _cached_test_cases));
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

        class Listener : MarshalByRefObject,INUFLTestEventListener
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
                _handle.RecordEnd(vs_test_case, vs_result.Outcome);
                
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

            public void OnTestStart(string fullname)
            {
                var vs_test_case = _cache[fullname];
                _handle.RecordStart(vs_test_case);
            }
        }
    }
}
