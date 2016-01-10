using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Setting;
using NUnit.Framework;
using NUFL.Framework.Model;
namespace NUFL.Framework.Test.Setting
{
    [TestFixture]
    public class SettingTests
    {
        [Test]
        public void SettingPersistTest()
        {
            NUFLSetting setting = new NUFLSetting();
            setting.SetBackup("./test.config", "");
            ProgramEntityFilter filter = new ProgramEntityFilter();
            filter.RawFilters = "+[*]*,-[NUFL*]*";
            setting.SetSetting("filter", filter);
            NUFLSetting setting2 = new NUFLSetting();
            setting2.SetBackup("./test.config", "");
            var fetched_filter = setting2.GetSetting<ProgramEntityFilter>("filter");
            
            
        }

        [Test]
        public void FileBackedSettingTest()
        {
            FileBackedSetting setting = new FileBackedSetting();
            setting.RegisterSetting("fl_method", "op1");
            setting.RegisterSetting("filter", new ProgramEntityFilter());
            setting.RegisterSetting("function_mode", "susp");
            setting.BackupFile = ".nufl.config";
            setting.SetSetting("fl_method", "test");

            var setting2 = new FileBackedSetting();
            setting2.RegisterSetting("fl_method", "op1");
            setting2.RegisterSetting("filter", new ProgramEntityFilter());
            setting2.RegisterSetting("background_mode", "susp");
            setting2.BackupFile = ".nufl.config";
            Assert.That(setting2.GetSetting("fl_method").Equals("test"));
        }

        [Test]
        public void NUFLSettingTest()
        {
            NUFLSetting setting = new NUFLSetting();
            setting.SetBackup(".nufl.config", "");
            Assert.That(setting.GetSetting<string>("fl_method").Equals("test"));
        }
    }
}
