using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUFL.Framework.Symbol;
using NUFL.Framework.Model;
using NUFL.Framework.Setting;
using log4net;

namespace NUFL.Framework.Test.Model
{
    [TestFixture]
    class InstrumentationModuleBuilderTests
    {
        NUFLOption _option;
        IFilter _filter;
        ILog _logger;
        IInstrumentationModelBuilder _module_builder;
        [SetUp]
        public void Setup()
        {
            _option = new NUFLOption();
            _option.TestAssemblies.Add(@".\MockAssembly\NUFL.TestTarget.dll");
            _option.TargetDir = @".\MockAssembly";
            _option.ProfileFilters.Add("+[*]*");
            _filter = Filter.BuildFilter(new List<string>(){@".\MockAssembly\NUFL.TestTarget.dll"});
            _logger = LogManager.GetLogger("test");
            _module_builder = new InstrumentationModelBuilder(@".\MockAssembly\NUFL.TestTarget.dll", "NUFL.TestTarget", _option, _filter, new List<string>());
        }
        [Test]
        public void ModuleBuilderSmokeTest()
        {
            Module module = _module_builder.BuildModuleModel();
            Assert.IsNotEmpty(module.Classes);
        }
        [Test]
        public void ModuleBuilderFileTest()
        {
            Module module = _module_builder.BuildModuleModel();
            foreach(var key in SourceFile.FileDict.Keys)
            {
                SourceFile file = SourceFile.GetSourceFile(key);
                Console.WriteLine(file.FullName);
                foreach(var method in file.Methods)
                {
                    Console.WriteLine(method.Name);
                }
            }
        }
    }
}
