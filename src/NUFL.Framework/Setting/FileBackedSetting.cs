using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.XPath;
namespace NUFL.Framework.Setting
{
    public class FileBackedSetting
    {
        string _backup_file;
        [XmlIgnore]
        public string BackupFile 
        {
            set
            {
                _backup_file = value;
                Load();
            }
            get
            {
                return _backup_file;
            }
        }

        Dictionary<string, object> _settings = new Dictionary<string, object>();
        Dictionary<string, object> Settings
        {
            get { return _settings; }
        }

        public FileBackedSetting()
        {
            BackupFile = null;
        }

        public void RegisterSetting(string name, object default_value)
        {
            Settings.Add(name, default_value);
        }

        public object GetSetting(string name)
        {
            return Settings[name];
        }

        public void SetSetting(string name, object value)
        {
            SetSettingWithoutPerisit(name, value);
            Persist();
            
        }

        void SetSettingWithoutPerisit(string name, object value)
        {
            if (!Settings.ContainsKey(name))
            {
                throw new Exception("Unregistered setting " + name);
            }
            if (Settings[name] == value)
            {
                return;
            }
            Settings[name] = value;
            OnSettingChanged(name, value);
        }

        public event Action<string, object> SettingChanged;

        void OnSettingChanged(string name, object value)
        {
            if(SettingChanged != null)
            {
                try
                {
                    SettingChanged(name, value);
                }
                catch (Exception) { }
            }
        }

        void Load()
        {
            try
            {
                using (FileStream fs = File.Open(_backup_file, FileMode.Open))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fs);
                    var root = doc.FirstChild;

                    foreach (XmlNode child in root.ChildNodes)
                    {
                        var key = child.Name;
                        var serializer = new XmlSerializer(Settings[key].GetType());
                        using(var reader = new XmlNodeReader(child.FirstChild))
                        {
                            var obj = serializer.Deserialize(reader);
                            SetSettingWithoutPerisit(key, obj);
                        }
                        
                     
                    }

                }
            }
            catch (Exception) { }
            
        }

        public void Persist()
        {
            if(BackupFile == null || BackupFile == "")
            {
                return;
            }
            try
            {
                using (FileStream fs = File.Open(_backup_file, FileMode.Create))
                {
                    XmlDocument doc = new XmlDocument();
                    var root = doc.CreateElement("setting");
                    doc.AppendChild(root);

                    foreach(var setting in Settings)
                    {
                        var node = doc.CreateElement(setting.Key);
                        root.AppendChild(node);
                        var serializer = new XmlSerializer(setting.Value.GetType());
                        var nav = node.CreateNavigator();
                        using (var writer = nav.AppendChild())
                        {
                            writer.WriteWhitespace(" ");
                            serializer.Serialize(writer, setting.Value);
                            writer.Close();
                        }
                    }

                    doc.Save(fs);
                    fs.Close();
                }
            }
            catch (Exception e) { }
        }
    }
}
