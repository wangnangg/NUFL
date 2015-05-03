using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.ComponentModel;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Services;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
namespace NUFL.Service
{
    public class ServiceManager
    {
        Dictionary<Type, IServiceContainer> _service_mapper;
        List<IServiceContainer> _services;
        int _port;
        TcpChannel _channel;
        public ServiceManager(int port)
        {
            _port = port;
            _service_mapper = new Dictionary<Type, IServiceContainer>();
            _services = new List<IServiceContainer>();
           
        }
        public void ProvideGlobalService(Type service_interface, GlobalServiceBase service)
        {
            var container = new GlobalServiceContainer(service, service_interface);
            _service_mapper.Add(service_interface, container);
            _services.Add(container);
        }
        public void RequireService(Type service_interface, int port)
        {
            var container = new RemoteServiceContainer(port, service_interface);
            _service_mapper.Add(service_interface, container);
            _services.Add(container);
        }
        public void Start()
        {
            _channel = ChannelHelper.GetTcpChannel(_port.ToString(), _port);
            foreach(var service in _services)
            {
                service.Initialize();
            }
        }

        public void Stop()
        {
            ChannelHelper.SafeReleaseChannel(_channel);
        }

        public object GetService(Type service_interface)
        {
            return _service_mapper[service_interface].GetService();
        }

     
    }
}

