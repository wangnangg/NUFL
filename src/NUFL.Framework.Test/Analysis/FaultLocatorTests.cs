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
using System.Diagnostics;
using NUFL.GUI.ViewModel;
using NUFL.Service;
using NUFL.Framework.Analysis;
using System.Collections.ObjectModel;
using NUFL.Framework.Model;
using NUFL.GUI.Model;
namespace NUFL.Framework.Test.Analysis
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
            var list = _fault_locator.GetRankList((x) => { return true; }, "op1");
            _test_runner.UnLoad();
            foreach(var item in list.GetSuspList(typeof(Method)))
            {
                Debug.WriteLine(item.DisplayName + " " + item.Susp);
            }
        }
        [Test]
        public void FaultLocatorClass()
        {
            _persistance = new FaultLocator();
            _fault_locator = (IFaultLocator)_persistance;
            _test_runner.Load(_option, _filter, _persistance);
            _test_runner.RunTests(NUnit.Engine.TestFilter.Empty);
            var list = _fault_locator.GetRankList((x) => { return true; }, "op1");
            _test_runner.UnLoad();
            foreach (var item in list.GetSuspList(typeof(Class)))
            {
                Debug.WriteLine(item.DisplayName + " " + item.Susp);
            }
        }
        [Test]
        public void FaultLocatorStatement()
        {
            _persistance = new FaultLocator();
            _fault_locator = (IFaultLocator)_persistance;
            _test_runner.Load(_option, _filter, _persistance);
            _test_runner.RunTests(NUnit.Engine.TestFilter.Empty);
            var list = _fault_locator.GetRankList((x) => { return true; }, "op1");
            _test_runner.UnLoad();
            foreach (var item in list.GetSuspList(typeof(InstrumentationPoint)))
            {
                Debug.WriteLine(item.DisplayName + " " + item.Susp);
            }
        }

        [Test]
        public void CoverageClass()
        {
            _persistance = new FaultLocator();
            _fault_locator = (IFaultLocator)_persistance;
            _test_runner.Load(_option, _filter, _persistance);
            _test_runner.RunTests(NUnit.Engine.TestFilter.Empty);
            var list = _fault_locator.GetRankList((x) => { return true; }, "op1");
            _test_runner.UnLoad();
            foreach (var item in list.GetCovList(typeof(Method)))
            {
                Debug.WriteLine(item.DisplayName + " " + item.CoveragePercent);
            }
        }
        [Test]
        public void CoverageProgram()
        {
            _persistance = new FaultLocator();
            _fault_locator = (IFaultLocator)_persistance;
            _test_runner.Load(_option, _filter, _persistance);
            _test_runner.RunTests(NUnit.Engine.TestFilter.Empty);
            var list = _fault_locator.GetRankList((x) => { return true; }, "op1");
            _test_runner.UnLoad();
            foreach (var item in list.GetCovList(typeof(Program)))
            {
                Debug.WriteLine(item.DisplayName + " " + item.CoveragePercent);
            }
        }

        [Test]
        public void FaultLocatorPresentation()
        {
            _persistance = new FaultLocator();
            _fault_locator = (IFaultLocator)_persistance;
            _test_runner.Load(_option, _filter, _persistance);
            _test_runner.RunTests(NUnit.Engine.TestFilter.Empty);
            var list = _fault_locator.GetRankList((x) => { return true; }, "op1");
            _test_runner.UnLoad();

            ServiceManager mgr = new ServiceManager(0);
            mgr.RequireService(typeof(IFLResultPresenter), 3245);
            mgr.Start();
            IFLResultPresenter presenter = mgr.GetService(typeof(IFLResultPresenter)) as IFLResultPresenter;
            presenter.Present(list);
            System.Threading.Thread.Sleep(5000);
        }



        [TearDown]
        public void TearDown()
        {

        }
    }
}
