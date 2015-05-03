using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Service
{
    class RemoteServiceContainer:IServiceContainer
    {
        string _uri;
        Type _type;
        public RemoteServiceContainer(int port, Type service_interface)
        {
            _uri = "tcp://127.0.0.1:" + port + "/" + service_interface.FullName;
            _type = service_interface;
        }



        public object GetService()
        {
            return Activator.GetObject(_type, _uri);
        }


        public void Initialize()
        {
            //nothing to do
        }
    }
}
