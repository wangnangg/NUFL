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
        FLResultPresenter presenter = new FLResultPresenter();
        FLResultViewModel view_model = new FLResultViewModel();
        NUFLOption option;
        public void Setup()
        {
            _result_view.DataContext = view_model;
            option = new NUFLOption();
            option.FLMethod = "op1";
            option.Filters = new List<string>() { "+[*]*" };
            presenter.ViewModel = view_model;
           // ServiceManager.Instance.RegisterGlobalInstanceService(typeof(IOption), option, "");
            //ServiceManager.Instance.RegisterGlobalInstanceService(typeof(IFLResultPresenter), presenter, "");  
            ServiceManager.Instance.RegisterGlobalInstanceService(typeof(IOption), option, "test");
            ServiceManager.Instance.RegisterGlobalInstanceService(typeof(IFLResultPresenter), presenter, "test");  



        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Setup();
        }
       


    }
}
