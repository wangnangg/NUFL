using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Setting;
using NUFL.Framework.Persistance;
using NUFL.Framework.Analysis;
namespace NUFL.Framework.TestRunner
{
    public interface IRunnerFactory
    {
        bool IsX64 { set; }
        int OwnerPid { set;  }
        INUFLTestRunner GetProcessRunner(Action<string, string> custome_launch);
        INUFLTestRunner GetProfileRunner();
        INUFLTestRunner GetRunner();
       
    }
    public class RunnerFactory:IRunnerFactory
    {
        public ISetting Option { set; get; }

        public INUFLTestRunner GetProcessRunner(Action<string, string> custom_launch)
        {
            return new ProcessRunner(IsX64, null, custom_launch);
        }

        public INUFLTestRunner GetProfileRunner()
        {
            var fault_locator = new FaultLocator()
            {
                Option = this.Option,
            };
            var runner = new ProfileRunner(
                IsX64,
                this.Option,
                fault_locator
            );
            return runner;
        }

        public INUFLTestRunner GetRunner()
        {
            if (!Option.GetSetting<bool>("collect_coverage"))
            {
                return GetProcessRunner(null);
            }
            return GetProfileRunner();
        }

        public bool IsX64 { set; get; }
        public int OwnerPid { set; get; }
    }
}
