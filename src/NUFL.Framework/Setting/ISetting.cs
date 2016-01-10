//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using System.Collections.Generic;
using NUFL.Framework.Symbol;
using NUFL.Framework.Model;
using NUFL.Framework.ProfilerCommunication;
using System;
using NUFL.Service;
namespace NUFL.Framework.Setting
{
    /// <summary>
    /// properties exposed by the command line object for use in other entities
    /// </summary>
    public interface ISetting
    {

        T GetSetting<T>(string name);
        event Action<string, object> SettingChanged;
        void SetSetting(string name, object value);
    }

    public class NUFLSetting : ISetting
    {
        FileBackedSetting _solution_setting = new FileBackedSetting();
        public NUFLSetting()
        {
            _solution_setting.RegisterSetting("fl_method", "op1");
            _solution_setting.RegisterSetting("show_background_color", false);
            _solution_setting.RegisterSetting("function_mode", "cov");
            _solution_setting.RegisterSetting("granularity_mode", "Class");
            _solution_setting.RegisterSetting("filter", new ProgramEntityFilter());
            _solution_setting.RegisterSetting("collect_coverage", false);
            _solution_setting.SettingChanged += OnSettingChanged;
        }
        public event Action<string, object> SettingChanged;

        public void SetBackup(string solution_file, string global_file)
        {
            _solution_setting.BackupFile = solution_file;
        }

        void OnSettingChanged(string key, object value)
        {
            if(SettingChanged != null)
            {
                SettingChanged(key, value);
            }
        }

        public void SetSetting(string name, object value)
        {
            _solution_setting.SetSetting(name, value);
        }


        public T GetSetting<T>(string name)
        {
            return (T)_solution_setting.GetSetting(name);
        }
    }
}