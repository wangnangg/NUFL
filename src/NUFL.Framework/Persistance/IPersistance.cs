using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Model;
using NUFL.Framework.ProfilerCommunication;
using NUFL.Framework.TestModel;
namespace NUFL.Framework.Persistance
{
    public interface IPersistance
    {
        void PersistProgram(Program program);
        void PersistTestResult(TestResult result);
        void SaveCoverageData(UInt32[] data, UInt32 length);
        void Commit();
    }
}
