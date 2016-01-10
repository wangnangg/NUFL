using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using NUFL.Framework.Setting;
using NUFL.Framework.Model;
namespace NUFL.GUI.ViewModel
{
    public class GranularityItem
    {
        public string ImagePath {set;get;}
        public Type Type {set;get;}
        public string Name
        {
            get
            {
                return Type.Name;
            }
        }
    }
    public class GranularityChoice:INotifyPropertyChanged
    {
        public int Index 
        { 
            set
            {
                _setting.SetSetting("granularity_mode", Items[value].Name);
            }
            get
            {
                string type = _setting.GetSetting<string>("granularity_mode");
                for(int i=0; i<Items.Count; i++)
                {
                    if(Items[i].Name == type)
                    {
                        return i;
                    }
                }
                return 0;
            }
        }

        public List<GranularityItem> Items { set; get; }

        ISetting _setting;
        public GranularityChoice(ISetting setting)
        {
            _setting = setting;
            GranularityItem program = new GranularityItem()
            {
                ImagePath = "/NUFL.GUI;Component/Images/Program.png",
                Type = typeof(Program),
            };
            GranularityItem module = new GranularityItem()
            {
                ImagePath = "/NUFL.GUI;Component/Images/Module.png",
                Type = typeof(Module),
            };
            GranularityItem @class = new GranularityItem()
            {
                ImagePath = "/NUFL.GUI;Component/Images/Class.png",
                Type = typeof(Class),
            };
            GranularityItem method = new GranularityItem()
            {
                ImagePath = "/NUFL.GUI;Component/Images/Method.png",
                Type = typeof(Method),
            };
            Items = new List<GranularityItem>() { program, module, @class, method };
            _setting.SettingChanged += _setting_SettingChanged;
        }

        void _setting_SettingChanged(string key, object value)
        {
            if(key == "granularity_mode")
            {
                OnPropertyChanged("Index");
                return;
            }
        }

        public Type GranularityType
        {
            get
            {
                return Items[Index].Type;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
