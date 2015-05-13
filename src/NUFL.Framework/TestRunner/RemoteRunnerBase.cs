using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.TestModel;
using System.Runtime.Remoting.Lifetime;

namespace NUFL.Framework.TestRunner
{
    public abstract class RemoteRunnerBase : MarshalByRefObject, IDisposable, ISponsor
    {

        public virtual void Dispose()
        {
            _disposed = true;
        }

        public override object InitializeLifetimeService()
        {
            ILease lease = (ILease)base.InitializeLifetimeService();
            if (lease.CurrentState == LeaseState.Initial)
            {
                lease.InitialLeaseTime = TimeSpan.FromMinutes(3);
                lease.SponsorshipTimeout = TimeSpan.FromMinutes(2);
                lease.RenewOnCallTime = TimeSpan.FromMinutes(2);
                lease.Register(this);
            }
            return lease;
        }

        bool _disposed = false;
        public TimeSpan Renewal(ILease lease)
        {
            if (_disposed)
            {
                return TimeSpan.Zero;
            }
            else
            {
                return TimeSpan.FromMinutes(2);
            }
        }
    }
}
