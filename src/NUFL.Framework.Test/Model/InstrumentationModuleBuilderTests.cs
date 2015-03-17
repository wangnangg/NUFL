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
        CommandLineParser _commandline;
        IFilter _filter;
        ILog _logger;
        InstrumentationModelBuilderFactory _module_builder_factory;
        IInstrumentationModelBuilder _module_builder;
        [SetUp]
        public void Setup()
        {
            _commandline = new CommandLineParser(new string[]{
                 "-target:NUFL.TestTarget.dll",
                 "-targetargs:",
                 "-register:user",
                 "-filter:+[*]*"
             });
            _commandline.ExtractAndValidateArguments();
            _filter = Filter.BuildFilter(_commandline);
            _logger = LogManager.GetLogger("test");
            _module_builder_factory = new InstrumentationModelBuilderFactory(_commandline, _filter, _logger);
            _module_builder = _module_builder_factory.CreateModelBuilder(".\\NUFL.TestTarget.dll", "NUFL.TestTarget");
        }
        [Test]
        public void ModuleBuilderSmokeTest()
        {
            Module module = _module_builder.BuildModuleModel(true);
            Assert.IsNotEmpty(module.Files);
            Assert.IsNotEmpty(module.Classes);
        }
    }
}
