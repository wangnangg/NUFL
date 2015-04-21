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
using NUFL.Framework.CBFL;
using NUFL.Framework.Model;
using NUFL.Framework.ProfilerCommunication;
using NUFL.Framework.TestModel;
using NUFL.Framework.NUnitTestFilter;

namespace NUFL.Framework.Test.TestRunner
{
    [TestFixture]
    class ProfileTestRunnerTests
    {
        NUFLOption _option;
        IFilter _filter;
        ILog _logger;
        IPersistance _persistance;
        [SetUp]
        public void Setup()
        {
            _option = new NUFLOption();
            _option.TestAssemblies.Add(@".\MockAssembly\NUFL.TestTarget.dll");
            _option.TargetDir = @".\MockAssembly";
            _option.ProfileFilters.Add("+[NUFL*]*");

            _filter = Filter.BuildFilter(_option.ProfileFilters, false);
            _logger = LogManager.GetLogger("test");
            
            _persistance = new FaultLocator();
            
  
           
        }
        [Test]
        public void ProfileTestRunnerSmoke()
        {
            var _test_runner = new ProfileTestRunner(_logger);
            _test_runner.Load(_option, _filter, _persistance);
            _test_runner.UnLoad();
            _test_runner.Load(_option, _filter, _persistance);
            _test_runner.UnLoad();
            _test_runner.Load(_option, _filter, _persistance);
            _test_runner.UnLoad();
            
           
        }
        [Test]
        public void ProfileTestRunnerRun()
        {
            var _test_runner = new ProfileTestRunner(_logger);
            _test_runner.Load(_option, _filter, _persistance);
            var container = _test_runner.RunTests(NUnit.Engine.TestFilter.Empty);
            _test_runner.UnLoad();
            foreach (var test_case in container.GetTestCaseEnumerator())
            {
                System.Diagnostics.Debug.WriteLine("{0}:{1}",test_case.FullName, test_case.Result);
            }
           
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public void ProfileTestRunnerFilterRun(int count)
        {
            var _test_runner = new ProfileTestRunner(_logger);
            _test_runner.Load(_option, _filter, _persistance);
            var discover_result = _test_runner.DiscoverTests(NUnit.Engine.TestFilter.Empty);
            List<int> test_ids = new List<int>();
            foreach (var test_case in discover_result.GetTestCaseEnumerator())
            {
                test_ids.Add(test_case.Id);
                count--;
                if (count == 0)
                    break;
            }
            IdFilter test_filter = new IdFilter(test_ids);
            var container = _test_runner.RunTests(test_filter);
            _test_runner.UnLoad();
            foreach (var test_case in container.GetTestCaseEnumerator())
            {
                System.Diagnostics.Debug.WriteLine("{0}:{1}", test_case.FullName, test_case.Result);
            }

        }
        [Test]
        public void ProfileTestRunnerDiscover()
        {
            var _test_runner = new ProfileTestRunner(_logger);
            _test_runner.Load(_option, _filter, _persistance);
            var container = _test_runner.DiscoverTests(NUnit.Engine.TestFilter.Empty);
            _test_runner.UnLoad();
            foreach(var test_case in container.GetTestCaseEnumerator())
            {
                System.Diagnostics.Debug.WriteLine(test_case.FullName);
            }
            
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}
