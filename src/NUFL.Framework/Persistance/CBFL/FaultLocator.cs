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

        public void SaveCoverageData(UInt32[] data, UInt32 length)
        {
            for(int i=0; i<length; i++)
            {
                Debug.Write(data[i]);
                Debug.Write(" ");
            }
        }


    }
}
