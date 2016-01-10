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
using System.Collections.ObjectModel;
using NUFL.Framework.Analysis;
using NUFL.Framework.Model;
using System.ComponentModel;
using NUFL.Framework.Setting;
using NUFL.Service;
namespace NUFL.GUI.ViewModel
{
    public class FLResultViewModel:INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ICommand _open_setting_cmd = null;
        public ICommand OpenSettingCommand
        {
            get
            {
                if(_open_setting_cmd == null)
                {
                    _open_setting_cmd = new NUFL.GUI.Command.OpenSettingCommand();
                }
                return _open_setting_cmd;
            }
        }

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }


        public ISetting Setting
        {
            get
            {
                return _setting;
            }
        }

        ISetting _setting;
        public FLResultViewModel(ISetting setting)
        {
            _setting = setting;
            

            this.Function = new FunctionChoice(_setting);

            this.Granularity = new GranularityChoice(_setting);

            GlobalEventManager.Instance.SubscribeEventByName(EventEnum.RankListChanged, OnRankListChanged);
            GlobalEventManager.Instance.SubscribeEventByName(EventEnum.StatusChanged, OnStatusChanged);
            _setting.SettingChanged += _setting_SettingChanged;
        }

        void OnRankListChanged(GlobalEvent @event)
        {
            DataSource = @event.Argument as RankList;
        }
        void OnStatusChanged(GlobalEvent @event)
        {
            Status = @event.Argument as string;
        }
        void _setting_SettingChanged(string key, object value)
        {
            if(key == "show_background_color")
            {
                OnPropertyChanged(new PropertyChangedEventArgs("ShowBackgroundColor"));
                return;
            }
            if(key == "collect_coverage")
            {
                OnPropertyChanged(new PropertyChangedEventArgs("CollectCoverage"));
                return;
            }
            if( key == "granularity_mode")
            {
                OnPropertyChanged(new PropertyChangedEventArgs("CovResult"));
                OnPropertyChanged(new PropertyChangedEventArgs("SuspResult"));
                return;
            }
        }

 
        public void Dispose()
        {
            GlobalEventManager.Instance.UnsubscribeEventByName(EventEnum.RankListChanged, OnRankListChanged);
            GlobalEventManager.Instance.UnsubscribeEventByName(EventEnum.StatusChanged, OnStatusChanged);
        }

        public IEnumerable<CovEntity> CovResult
        {
            get
            {
                if (DataSource == null)
                {
                    yield break;
                }
                Type gran = Granularity.GranularityType;
                if (gran == null)
                {
                    yield break;
                }
                foreach (var item in DataSource.GetCovList(gran))
                {
                    yield return new CovEntity(item);
                }
                yield break;
            }
        }

        string _status = "Ready";
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Status"));
            }
        }

        public IEnumerable<SuspEntity> SuspResult
        {
            get
            {
                if (DataSource == null)
                {
                    yield break;
                }
                Type gran = Granularity.GranularityType;
                if (gran == null)
                {
                    yield break;
                }
                foreach(var item in DataSource.GetSuspList(gran))
                {
                    yield return new SuspEntity(item);
                }
                yield break;
            }
        }


        public GranularityChoice Granularity
        {
            set;
            get;
        }

        public FunctionChoice Function
        {
            set;
            get;
        }

        public bool CollectCoverage
        {
            set
            {
                _setting.SetSetting("collect_coverage", value);
            }
            get
            {
                return _setting.GetSetting<bool>("collect_coverage");
            }
        }

        public bool ShowBackgroundColor
        {
            set
            {
                _setting.SetSetting("show_background_color", value);
            }
            get
            {
                return _setting.GetSetting<bool>("show_background_color");
            }
        }

        RankList _data_source;
        public RankList DataSource 
        { 
            set
            {
                _data_source = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SuspResult"));
                OnPropertyChanged(new PropertyChangedEventArgs("CovResult"));
            }
            get
            {
                return _data_source;
            }
        }




    }
}
