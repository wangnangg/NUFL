using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Runtime;
using System.Xml;
using NUFL.Framework;
using System.Windows.Markup;
using NUFL.Framework.TestRunner;
using NUFL.Framework.Setting;
using log4net;
using NUFL.Framework.Persistance;
using NUFL.Framework.Analysis;
using NUFL.Framework.Model;
using NUFL.Framework.ProfilerCommunication;
using NUFL.Framework.TestModel;
using NUFL.Framework.NUnitTestFilter;
using NUnit.Engine;
using System.Threading;
using NUFL.GUI.Model;
using NUFL.GUI.ViewModel;
using NUFL.Service;
namespace NUFL.GUI.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        NUFLOption _option;
        IFilter _filter;
        ILog _logger;
        IFaultLocator _fault_locator;
        IPersistance _persistance;
        ProfileTestRunner _test_runner;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadStart ts = new ThreadStart(worker);
            Thread tr = new Thread(ts);
            tr.Start();

            FLResultPresentService service = new FLResultPresentService(this.Dispatcher);
            ServiceManager mgr = new ServiceManager(3245);
            mgr.ProvideGlobalService(typeof(IFLResultPresenter), service);
            mgr.Start();
            _result_view.DataContext = service.ViewModel;

        }
        void worker()
        {
            Setup();
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
        }


    }
}
