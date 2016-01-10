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
using System.ComponentModel;
using NUFL.Framework.Setting;
namespace NUFL.GUI.ViewModel
{
    public class FunctionItem
    {
        public string ImagePath { set; get; }
        public string Name { set; get; }
        public string FullName { set; get; }
    }
    public class FunctionChoice:INotifyPropertyChanged
    {
        public List<FunctionItem> Items { set; get; }
        public int Index 
        { 
            set
            {
               _setting.SetSetting("function_mode", Items[value].FullName);
            }
            get
            {
                var result = _setting.GetSetting<string>("function_mode");
                if(result == "cov")
                {
                    return 0;
                } else
                {
                    return 1;
                }
            }
        }

        public Visibility CovVisible
        {
            get
            {
                return Index == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility SuspVisible
        {
            get
            {
                return Index == 1 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        ISetting _setting;

        public FunctionChoice(ISetting setting)
        {
            _setting = setting;
            _setting.SettingChanged += _setting_SettingChanged;
            FunctionItem cov = new FunctionItem() { Name = "Coverage" , FullName="cov"};
            FunctionItem susp = new FunctionItem() { Name = "Fault Localization", FullName="susp" };
            Items = new List<FunctionItem>(){cov, susp};
        }

        void _setting_SettingChanged(string key, object value)
        {
            if(key == "function_mode")
            {
                OnPropertyChanged("Index");
                OnPropertyChanged("CovVisible");
                OnPropertyChanged("SuspVisible");
            }
        }

        public string Choice
        {
            get
            {
                return Items[Index].FullName;
            }
        }

        public void OnPropertyChanged(string name)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
