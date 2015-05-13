using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Server
{
    public interface IServerSubscription
    {
        void Subscribe(Guid guid);

        void UnSubscribe(Guid guid);
    }
}
