using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Services;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace NUFL.Service
{
    class GlobalServiceContainer:IServiceContainer
    {
        GlobalServiceBase _instance;
        Type _interface;
        string _uri;
        public GlobalServiceContainer(GlobalServiceBase instance, Type service_interface)
        {
            _instance = instance;
            _interface = service_interface;
            _uri = _interface.FullName;
        }

        public object GetService()
        {
            return _instance;
        }

        public void Initialize()
        {
            RemotingServices.Marshal(_instance, _uri);
        }
    }
}
