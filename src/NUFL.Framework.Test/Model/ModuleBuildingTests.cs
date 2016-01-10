using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Model;
using NUnit.Framework;
using NUFL.Framework.Setting;   
namespace NUFL.Framework.Test.Model
{
    [TestFixture]
    class ModuleBuildingTests
    {
        string target = @".\MockAssembly\NUFL.TestTarget.dll";
        NUFLSetting option = new NUFLSetting();
        ProgramEntityFilter filter = new ProgramEntityFilter();
        Program program;
        [SetUp]
        public void SetUp()
        {
            program = new Program(option.GetSetting<ProgramEntityFilter>("filter"), new List<string>());
        }
        [Test]
        public void BuidProgramSmoke()
        {
            program.AddModule(target, "NUFL.TestTarget.dll");
        }
    }
}
