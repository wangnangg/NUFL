using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.TestRunner;
using NUnit.Framework;
using NUFL.Framework.TestModel;
using System.Diagnostics;
namespace NUFL.Framework.Test.TestRunner
{
    [TestFixture]
    public class TestRunnerTests
    {
        string[] assemblies = new string[] { @".\MockAssembly\NUFL.TestTarget.dll" };

        [Test]
        public void NUnitTestRunnerWrapper_RunAll()
        {
            List<TestCase> test_cases;
            using (INUFLTestRunner runner = new NUnitTestRunnerWrapper())
            {
                runner.Load(assemblies);
                test_cases = runner.DiscoverTests();
            }

            foreach (var tc in test_cases)
            {
                Debug.WriteLine(tc.FullyQualifiedName);
                foreach (var prop in tc.Properties)
                {
                    Debug.WriteLine(prop.Item1 + " " + prop.Item2);
                }
            }
        }


        [Test]
        public void ProcessRunner_RunAll()
        {
            var runner_factory = new RunnerFactory();
            runner_factory.IsX64 = false;
            runner_factory.OwnerPid = Process.GetCurrentProcess().Id;
            INUFLTestRunner runner = runner_factory.GetProcessRunner(null);
                runner.Load(assemblies);
                var test_cases = runner.DiscoverTests();
                runner.RunTests(null, new Listener());
            runner.Dispose();

            foreach(var tc in test_cases)
            {
                Debug.WriteLine(tc.FullyQualifiedName);
                foreach(var prop in tc.Properties)
                {
                    Debug.WriteLine(prop.Item1 + " " + prop.Item2);
                }
            }
        }

        [Test]

        public void ProfilerRunner_RunAll()
        {
            var runner_factory = RemoteRunnerFactory.GetRemoteRunnerFactory("test");
            runner_factory.IsX64 = false;
            runner_factory.OwnerPid = Process.GetCurrentProcess().Id;
            using (var runner = runner_factory.GetProfileRunner())
            {
                runner.Load(assemblies);
                runner.RunTests(null, new Listener());
            }
        }

        [Test]
        public void WrapperRunner_Discover()
        {
            List<TestCase> test_cases;
            using (NUnitTestRunnerWrapper runner = new NUnitTestRunnerWrapper())
            {
                runner.Load(assemblies);
                test_cases = runner.DiscoverTests();
            }
            foreach (var tc in test_cases)
            {
                Debug.WriteLine(tc.FullyQualifiedName);
                foreach (var prop in tc.Properties)
                {
                    Debug.WriteLine(prop.Item1 + " " + prop.Item2);
                }
            }
        }

        class Listener : MarshalByRefObject, INUFLTestEventListener
        {
            public override object InitializeLifetimeService()
            {
                return null;
            }
            public void OnTestResult(TestResult result)
            {
                Debug.WriteLine(result.FullyQualifiedName);
                Debug.WriteLine(result.Outcome.ToString());
                Debug.WriteLine(result.ErrorMessage);
                Debug.WriteLine(result.StackTrace);
                Debug.WriteLine(result.Duration.Milliseconds);
            }

            public void OnTestStart(string fullname)
            {
                Debug.WriteLine("start " + fullname);
            }
        }
    }
}
