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
using System.Threading;
using NUFL.GUI.ViewModel;
using NUFL.Service;
namespace NUFL.GUI.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FLResultViewModel view_model;
        RemoteRunnerFactory _runner_factory;
        public void Setup()
        {
            var Option = new NUFLSetting();
            Option.SetSetting("collect_coverage", true);
            ServiceManager.Instance.RegisterLocalService(typeof(ISetting), Option);
            _runner_factory = new RemoteRunnerFactory() 
            {
                Option = Option,
                
            };
            ServiceManager.Instance.RegisterGlobalService(typeof(IRunnerFactory), _runner_factory, "test");

            view_model = new FLResultViewModel(Option);
            _result_view.DataContext = view_model;
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Setup();
        }


        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
          
        }

        private void CommandBinding_Executed_1(object sender, ExecutedRoutedEventArgs e)
        {

        }
       


    }
}
