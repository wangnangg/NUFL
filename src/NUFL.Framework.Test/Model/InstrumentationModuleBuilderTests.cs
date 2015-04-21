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
        InstrumentationModelBuilderFactory _module_builder_factory;
        IInstrumentationModelBuilder _module_builder;
        [SetUp]
        public void Setup()
        {
            _option = new NUFLOption();
            _option.TestAssemblies.Add(@".\MockAssembly\NUFL.TestTarget.dll");
            _option.TargetDir = @".\MockAssembly";
            _option.ProfileFilters.Add("+[*]*");
            _filter = Filter.BuildFilter(_option.ProfileFilters, false);
            _logger = LogManager.GetLogger("test");
            _module_builder_factory = new InstrumentationModelBuilderFactory(_option, _filter, _logger);
            _module_builder = _module_builder_factory.CreateModelBuilder(".\\NUFL.TestTarget.dll", "NUFL.TestTarget");
        }
        [Test]
        public void ModuleBuilderSmokeTest()
        {
            Module module = _module_builder.BuildModuleModel();
            Assert.IsNotEmpty(module.Classes);
        }
    }
}
