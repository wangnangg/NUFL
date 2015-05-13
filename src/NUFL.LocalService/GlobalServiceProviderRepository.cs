using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace NUFL.Service
{
    public class GlobalServiceProviderRepository:IDisposable
    {
        string _unique_name;
        MemoryMappedFile _file;
        MemoryMappedViewStream _stream;
        uint _file_size = 4096 * 4;
        EventWaitHandle _lock;
        public GlobalServiceProviderRepository(string unique_name)
        {
            _unique_name = unique_name;
            string file_name = _unique_name + "#file";
            _file = MemoryMappedFile.CreateOrOpen(file_name, _file_size, MemoryMappedFileAccess.ReadWrite);
            _stream = _file.CreateViewStream(0, _file_size, MemoryMappedFileAccess.ReadWrite);
            string lock_name = unique_name + "#lock";
            _lock = new EventWaitHandle(true, EventResetMode.ManualReset, lock_name);
        }

        public void Remove(string fullname)
        {
            using (ServiceProviderDict dict = new ServiceProviderDict(_stream, _lock))
            {
                dict.Remove(fullname);
            }
        }


        public int this[string fullname]
        {
            set
            {
                using (ServiceProviderDict dict = new ServiceProviderDict(_stream, _lock))
                {
                    dict[fullname] = value;
                }
            }

            get
            {
                using (ServiceProviderDict dict = new ServiceProviderDict(_stream, _lock))
                {
                    return dict[fullname];
                }
            }
        }


        public void Dispose()
        {
            _stream.Close();
            _stream.Dispose();
            _file.Dispose();
            _lock.Dispose();
        }
    }
}
