using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Lifetime;
using System.Diagnostics;
namespace NUFL.Service
{
    public class GlobalProcessService : GlobalService, IDisposable, ISponsor
    {
        Process _owner_process;
        public GlobalProcessService(int owner_pid)
        {
            _owner_process = Process.GetProcessById(owner_pid);
        }
        public GlobalProcessService(Process owner_process)
        {
            _owner_process = owner_process;
           // _owner_process.Exited += _owner_process_Exited;
        }


        public override object InitializeLifetimeService()
        {
            var lease = (ILease)base.InitializeLifetimeService();
            if (lease.CurrentState == LeaseState.Initial)
            {
                lease.InitialLeaseTime = TimeSpan.FromSeconds(1);
                lease.SponsorshipTimeout = TimeSpan.FromSeconds(1);
                lease.RenewOnCallTime = TimeSpan.FromSeconds(1);
                lease.Register(this);
            }          
            return lease;
        }

        TimeSpan LastLife = TimeSpan.FromSeconds(1);
        public TimeSpan Renewal(ILease lease)
        {
            if (_disposed)
            {
                TimeSpan time = LastLife;
                LastLife = TimeSpan.Zero;
                return time;
            } else if(_owner_process.HasExited)
            {
                Dispose();
            }
            return TimeSpan.FromSeconds(1);
        }

        bool _disposed = false;
        public virtual void Dispose()
        {
            if(_disposed)
            {
                return;
            }
            System.Diagnostics.Debug.WriteLine("Process service disposing.");
            _disposed = true;
            OnOutOfScope();
        }


        void OnOutOfScope()
        {
            if (OutOfScope != null)
            {
                OutOfScope();
            }
        }

        public event Action OutOfScope;
    }
}
