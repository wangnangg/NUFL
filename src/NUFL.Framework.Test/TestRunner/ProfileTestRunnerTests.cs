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
        string[] assemblies = new string[] { @".\MockAssembly\NUFL.TestTarget.dll" };
        Guid guid = Guid.NewGuid();
        RemoteRunnerFactory _runner_factory;
        [TestFixtureSetUp]
        public void Setup()
        {
            _runner_factory.Key = "test";
            _runner_factory = (RemoteRunnerFactory)ServiceManager.Instance.GetService(typeof(ITestRunnerFactory), "test");

            NUFLOption option = new NUFLOption();
            option.FLMethod = "op1";
            option.Filters = new List<string>() { "+[NUFL*]*" };
            Service.ServiceManager.Instance.RegisterGlobalInstanceService(typeof(IOption), option, "test");
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

        class Listener:MarshalByRefObject,ITestResultListener
        {
            
            public void OnTestResult(TestResult result)
            {
                Debug.WriteLine(result.FullyQualifiedName);
                Debug.WriteLine(result.Outcome.ToString());
                Debug.WriteLine(result.ErrorMessage);
                Debug.WriteLine(result.StackTrace);
                Debug.WriteLine(result.Duration.Milliseconds);
            }
        }

    }
}
