using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.TestRunner;
using System.Threading;
using NUFL.Service;
using System.Diagnostics;
using System.Windows;
namespace NUFL.Agent
{
    class Program
    {
        static void Main(string[] args)
        {
           // MessageBox.Show("hehe");
            string key = args[0];
            int pid = int.Parse(args[1]);
            var _terminate_process = new EventWaitHandle(false, EventResetMode.ManualReset);
            RemoteRunner test_runner = new RemoteRunner(new NUnitTestRunnerWrapper(), pid);
            ServiceManager.Instance.RegisterGlobalService(typeof(INUFLTestRunner), test_runner, key);
            test_runner.OutOfScope += () =>
            {
                Console.WriteLine("Out of Scope.");
                _terminate_process.Set();
            };
            _terminate_process.WaitOne();
            Console.WriteLine("Shutting Down.");
            Thread.Sleep(200);
            ServiceManager.Instance.UnregisterGlobalService(typeof(INUFLTestRunner), key);
            Process.GetCurrentProcess().Kill();
        }

    }
}
