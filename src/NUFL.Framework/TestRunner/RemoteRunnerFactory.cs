using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NUFL.Service;
using System.Diagnostics;
using NUFL.Framework.Setting;
using NUFL.Framework.Persistance;
using NUFL.Framework.Analysis;
namespace NUFL.Framework.TestRunner
{
    public class RemoteRunnerFactory :GlobalSingletonService, IRunnerFactory
    {
        public static IRunnerFactory GetRemoteRunnerFactory(string key)
        {
            return (IRunnerFactory)ServiceManager.Instance.GetService(typeof(IRunnerFactory), key);
        }

        RunnerFactory _inner_factory;
        public ISetting Option
        {
            set
            {
                _inner_factory.Option = value;
            }
        }

        public RemoteRunnerFactory()
        {
            _inner_factory = new RunnerFactory();
        }


        public INUFLTestRunner GetProcessRunner(Action<string, string> custom_launch)
        {
            return new RemoteRunner(_inner_factory.GetProcessRunner(custom_launch), _owner_pid);
        }

        public INUFLTestRunner GetProfileRunner()
        {
            return new RemoteRunner(_inner_factory.GetProfileRunner(), _owner_pid);
        }

        public INUFLTestRunner GetRunner()
        {
            return new RemoteRunner(_inner_factory.GetRunner(), _owner_pid);
        }

        public bool IsX64
        {
            set
            {
                _inner_factory.IsX64 = value;
            }
        }
        int _owner_pid;
        public int OwnerPid
        {
            set
            {
                _owner_pid = value;
            }
        }
    }
}
