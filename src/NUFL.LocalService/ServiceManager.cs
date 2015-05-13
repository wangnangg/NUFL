using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.ComponentModel;
using System.Runtime.Remoting.Services;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
namespace NUFL.Service
{
    public class ServiceManager:IDisposable
    {
        Dictionary<string, MarshalByRefObject> _local_service_instance;
        TcpChannel _channel;
        int _max_connection = 5;
        GlobalServiceProviderRepository _repository;
        int _port = 0;
        public ServiceManager(string repository_name)
        {
            _local_service_instance = new Dictionary<string, MarshalByRefObject>();
            _repository = new GlobalServiceProviderRepository(repository_name);
        }
        static ServiceManager _instance = null;
        public static ServiceManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new ServiceManager("NUFL.Service.GlobalServiceProviderRepository");
                }
                return _instance;
            }
        }
        void Online()
        {
            if(_channel != null)
            {
                return;
            }
            _channel = ChannelHelper.GetTcpChannel(_max_connection);
            ChannelDataStore store = _channel.ChannelData as ChannelDataStore;
            string channelUri = store.ChannelUris[0];
            _port = int.Parse(channelUri.Substring(channelUri.LastIndexOf(':') + 1));
            
        }

        public void RegisterGlobalInstanceService(Type service_interface, GlobalInstanceServiceBase instance, string nmspc = "")
        {
            Online();
            string fullname = GetFullName(nmspc, service_interface);
            _local_service_instance[fullname] = instance;
            RemotingServices.Marshal(instance, fullname);
            //report on the global repository
            _repository[fullname] = _port;
            Console.WriteLine("Provide {0} service on {1}", service_interface.FullName, _port);
        }

        private string GetFullName(string nmspc, Type service_interface)
        {
            return nmspc + "." + service_interface.FullName;
        }

        public void UnregisterGlobalService(Type service_interface, string nmspc = "")
        {
            Online();
            string fullname = GetFullName(nmspc, service_interface);
            if (_local_service_instance.ContainsKey(fullname))
            {
                //report on the global repository
                _repository.Remove(fullname);

                var obj = _local_service_instance[fullname];
                _local_service_instance.Remove(fullname);
                RemotingServices.Disconnect(obj);
                
            }
        }

        public object GetService(Type service_interface, string nmspc = "")
        {
            string fullname = GetFullName(nmspc, service_interface);
            if (_local_service_instance.ContainsKey(fullname))
            {
                return _local_service_instance[fullname];
            }
            //find in repository
            Online();
            GlobalInstanceServiceBase proxy = null;
            int provider_port = _repository[fullname];
            if(provider_port > 0)
            {
                string uri = "tcp://127.0.0.1:" + provider_port + "/" + fullname;
                try
                {
                    //  proxy = (GlobalServiceBase)RemotingServices.Connect(service_interface, uri);
                    proxy = (GlobalInstanceServiceBase)Activator.GetObject(service_interface, uri);
                    proxy.Confirmation();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    proxy = null;
                }
            }
            if (proxy != null)
            {
                //we found the service
                return proxy;
            }
            throw new Exception("Service Not Found for " + service_interface.FullName);
        }



        public void Dispose()
        {
            if(_channel != null)
            {
                ChannelHelper.SafeReleaseChannel(_channel);
            }
            foreach(var key in _local_service_instance.Keys)
            {
                _repository.Remove(key);
                var obj = _local_service_instance[key];
                //its important that we disconnect all the objects;
                RemotingServices.Disconnect(obj);
            }
            _repository.Dispose();
        }
    }
}

