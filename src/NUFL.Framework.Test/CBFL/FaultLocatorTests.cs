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
using System.Diagnostics;
namespace NUFL.Framework.Test.CBFL
{
    [TestFixture]
    class FaultLocatorTests
    {
        NUFLOption _option;
        IFilter _filter;
        ILog _logger;
        IFaultLocator _fault_locator;
        IPersistance _persistance;
        ProfileTestRunner _test_runner;
        [SetUp]
        public void Setup()
        {
            _option = new NUFLOption();
            _option.TestAssemblies.Add(@".\MockAssembly\NUFL.TestTarget.dll");
            _option.TargetDir = @".\MockAssembly";
            _option.ProfileFilters.Add("+[NUFL*]*");
            _filter = Filter.BuildFilter(_option.ProfileFilters, false);
            _logger = LogManager.GetLogger("test");
            _test_runner = new ProfileTestRunner(_logger);
          
        }

        [Test]
        public void FaultLocatorMethod()
        {
            _persistance = new FaultLocator();
            _fault_locator = (IFaultLocator)_persistance;
            _test_runner.Load(_option, _filter, _persistance);
            _test_runner.RunTests(NUnit.Engine.TestFilter.Empty);
            var list = _fault_locator.GetRankList(Granularity.Method, (x) => { return true; }, "op1");
            _test_runner.UnLoad();
            foreach(var item in list)
            {
                Debug.WriteLine(item.Name + " " + item.susp);
            }
        }
        [Test]
        public void FaultLocatorClass()
        {
            _persistance = new FaultLocator();
            _fault_locator = (IFaultLocator)_persistance;
            _test_runner.Load(_option, _filter, _persistance);
            _test_runner.RunTests(NUnit.Engine.TestFilter.Empty);
            var list = _fault_locator.GetRankList(Granularity.Class, (x) => { return true; }, "op1");
            _test_runner.UnLoad();
            foreach (var item in list)
            {
                Debug.WriteLine(item.Name + " " + item.susp);
            }
        }
        [Test]
        public void FaultLocatorStatement()
        {
            _persistance = new FaultLocator();
            _fault_locator = (IFaultLocator)_persistance;
            _test_runner.Load(_option, _filter, _persistance);
            _test_runner.RunTests(NUnit.Engine.TestFilter.Empty);
            var list = _fault_locator.GetRankList(Granularity.Statement, (x) => { return true; }, "op1");
            _test_runner.UnLoad();
            foreach (var item in list)
            {
                Debug.WriteLine(item.Name + " " + item.susp);
            }
        }



        [TearDown]
        public void TearDown()
        {

        }
    }
}
