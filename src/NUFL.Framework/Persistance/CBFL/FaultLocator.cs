using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using NUFL.Framework.ProfilerCommunication;

namespace NUFL.Framework.Persistance.CBFL
{
    public class FaultLocator:IPersistance
    {
        IPCStream _profiler_data_stream;

        //保存
        public void PersistModule(Model.Module module)
        {
            Debug.WriteLine("persisting " + module.FullName);  
        }

        //清空intrument pionts 的visit count
        public void PersistTestResult(string xml_result)
        {
            Debug.WriteLine(xml_result);  
        }

        public void ConnectDataStream(IPCStream stream)
        {
            _profiler_data_stream = stream;
            ThreadStart ts = new ThreadStart(ProcessCovData);
            Thread tr = new Thread(ts);
            tr.Start();
        }
        private void ProcessCovData()
        {
            UInt32 read_size = _profiler_data_stream.BufferSize / 4 * 4;
            byte[] data = new byte[read_size];

            UInt32 actual_read_size = _profiler_data_stream.Read(data, 0, read_size);
            while (actual_read_size > 0)
            {
                //do something with data and actual_read_size;
                Debug.WriteLine("Coverage Data Received.");
                actual_read_size = _profiler_data_stream.Read(data, 0, read_size);
            }
        }
    }
}
