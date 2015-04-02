using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Model;
using NUFL.Framework.ProfilerCommunication;

namespace NUFL.Framework.Persistance
{
    public interface IPersistance
    {
        void PersistModule(Module module);
        void PersistTestResult(string xml_result);
        void SaveCoverageData(UInt32[] data, UInt32 length);
    }
}
