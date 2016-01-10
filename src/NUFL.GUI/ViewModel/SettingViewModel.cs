using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Setting;
using System.Windows;
using System.ComponentModel;

namespace NUFL.GUI.ViewModel
{
    public class SettingViewModel:INotifyPropertyChanged
    {
        ISetting _setting;
        public SettingViewModel(ISetting setting)
        {
            _setting = setting;
        }
        public string RawFilters
        {
            set
            {
                ProgramEntityFilter filter = new ProgramEntityFilter();
                try
                {
                    filter.RawFilters = value;
                } catch(Exception e)
                {
                    OnPropertyChanged("RawFilters");
                    MessageBox.Show(e.Message);
                    return;
                }
                _setting.SetSetting("filter", filter);
            }
            get
            {
                return _setting.GetSetting<ProgramEntityFilter>("filter").RawFilters;
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
