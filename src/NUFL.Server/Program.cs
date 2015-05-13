using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.TestRunner;
using NUFL.Framework.Analysis;
using System.Diagnostics;
using NUFL.Framework.Setting;

namespace NUFL.Server
{
    public class Program
    {
        static ServerSubscription subscription;
        static RemoteRunnerFactory runner_factory;
        static void Main(string[] args)
        {
            runner_factory = new RemoteRunnerFactory();
            Service.ServiceManager.Instance.RegisterGlobalInstanceService(typeof(ITestRunnerFactory), runner_factory);




            Guid guid = Guid.Parse(args[0]);
            subscription = new ServerSubscription(guid);
            Service.ServiceManager.Instance.RegisterGlobalInstanceService(typeof(IServerSubscription), subscription);

            subscription.NoSubscriber.WaitOne();

            System.Threading.Thread.Sleep(1000);

            Service.ServiceManager.Instance.Dispose();

            System.Threading.Thread.Sleep(2000);

            
        }
        public static bool StartSever(Guid guid)
        {
            bool succeed = true;
            try
            {
                var subscription = Service.ServiceManager.Instance.GetService(typeof(IServerSubscription)) as IServerSubscription;
                subscription.Subscribe(guid);

            }
            catch (Exception e)
            {
                succeed = false;
                Debug.WriteLine(e.Message);
                string path = typeof(Program).Assembly.Location;
                Process p = new System.Diagnostics.Process();
                ProcessStartInfo start_info = new System.Diagnostics.ProcessStartInfo(path, guid.ToString());
                start_info.UseShellExecute = true;
                p.StartInfo = start_info;
                p.Start();
            }
            if(succeed)
            {
                return true;
            }

            int poll_time = 100;
            int max_time = 3000;
            int wait_time = 0;
            succeed = false;
            while (wait_time < max_time)
            {
                try
                {
                    var subscription = Service.ServiceManager.Instance.GetService(typeof(IServerSubscription)) as IServerSubscription;
                    succeed = true;
                    break;
                } catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                    System.Threading.Thread.Sleep(poll_time);
                    wait_time += poll_time;
                    continue;
                }
            }
            return succeed;
                
        }

        public static void ShutdownServer(Guid guid)
        {
            var subscription = Service.ServiceManager.Instance.GetService(typeof(IServerSubscription)) as IServerSubscription;
            subscription.UnSubscribe(guid);
        }
    }
}
