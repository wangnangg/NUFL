using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using NUFL.Framework.ProfilerCommunication;
using System.Xml;

namespace NUFL.Framework.Persistance.CBFL
{
    public class FaultLocator:IPersistance
    {
        List<TestCase> _tc_list = new List<TestCase>();
        List<Coverage> _cov_list = new List<Coverage>();


        //保存
        public void PersistModule(Model.Module module)
        {
            Debug.WriteLine("persisting " + module.FullName);  
        }

        public void PersistTestResult(string xml_result)
        {
            var tc = TestCase.ParseFromXmlString(xml_result);
            if(tc != null)
            {
                _tc_list.Add(tc);
            }
        }

        Coverage current;
        public void SaveCoverageData(UInt32[] data, UInt32 length)
        {
            for (int i = 0; i < length; i++)
            {
                UInt32 uid = data[i];
                Debug.WriteLine(uid);
                if((uid & (UInt32)MSG_IdType.IT_MethodEnter) > 0)
                {
                    //this is method enter
                    current = new Coverage();
                    _cov_list.Add(current);
                    continue;
                }
                if((uid & (UInt32)MSG_IdType.IT_MethodLeave) > 0)
                {
                    //this is method leave
                    current.Complete = true;
                    current = null;
                    continue;
                }

                if((uid & (UInt32)MSG_IdType.IT_Mask) > 0)
                {
                    //this is uid
                    current.Cover(uid);
                    continue;
                }
            }
        }

        public void Commit()
        {
            foreach(var cov in _cov_list)
            {
                Debug.WriteLine("test-case:");
                for(UInt32 i=0; i<cov.Count; i++)
                {
                    Debug.WriteLine(i + ":" + cov.IsCovered(i));
                }
            }
        }


    }
}
