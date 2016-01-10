using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUFL.Framework.TestRunner;
using NUFL.Framework.Setting;
using log4net;
using NUFL.Framework.Persistance;
using NUFL.Framework.Analysis;
using NUFL.Framework.Model;
using NUFL.Framework.ProfilerCommunication;
using NUFL.Framework.TestModel;

using NUFL.Service;
using System.Diagnostics;

namespace NUFL.Framework.Test.TestRunner
{
    [TestFixture]
    class ProfileTestRunnerTests
    {
        //string[] assemblies = new string[] { @"E:\Users\Administrator\Desktop\Download\mathnet-numerics\out\test-debug\Net40\MathNet.Numerics.UnitTests.dll" };
        string[] assemblies = new string[] { @".\MockAssembly\NUFL.TestTarget.dll" };
        Guid guid = Guid.NewGuid();
        ITestRunnerFactory _runner_factory;
        [TestFixtureSetUp]
        public void Setup()
        {
            _runner_factory = (ITestRunnerFactory)ServiceManager.Instance.GetService(typeof(ITestRunnerFactory), "test");
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
        }
        [Test]
        public void ProfileTestRunnerSmoke()
        {
            using (var _test_runner = _runner_factory.CreateExecutor())
            {
                _test_runner.Load(assemblies);
                _test_runner.Unload();
                _test_runner.Load(assemblies);
                _test_runner.Unload();
                _test_runner.Load(assemblies);
                _test_runner.Unload();
                _test_runner.Dispose();
            }

        }
        [Test]
        public void ProfileTestRunnerRuntests()
        {
            using (var _test_runner = _runner_factory.CreateExecutor())
            {
                _test_runner.Load(assemblies);
                _test_runner.RunAllTests(new Listener());
                _test_runner.Unload();
            }
        
        }

        [Test]
        public void TestRunnerDiscovertests()
        {
            List<TestCase> tests;
            using (var _test_runner = new SimpleRunnerFactory().CreateDiscoverer())
            {
                _test_runner.Load(assemblies);
                tests = _test_runner.DiscoverTests();
                _test_runner.Unload();
            }

            foreach(var test in tests)
            {
                Console.WriteLine(test.DisplayName);
            }

        }

        class Listener:MarshalByRefObject,INUFLTestEventListener
        {
            
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
                throw new NotImplementedException();
            }
        }

    }
}
