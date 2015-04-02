using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUFL.Framework.TestRunner;
using NUFL.Framework.Setting;
using log4net;
using NUFL.Framework.Persistance;
using NUFL.Framework.Persistance.CBFL;
using NUFL.Framework.Model;
using NUFL.Framework.ProfilerCommunication;

namespace NUFL.Framework.Test.TestRunner
{
    [TestFixture]
    class ProfileTestRunnerTests
    {
        CommandLineParser _commandline;
        IFilter _filter;
        ILog _logger;
        ProfileTestRunner _test_runner;
        [SetUp]
        public void Setup()
        {
           
            _commandline = new CommandLineParser(new string[]{
                 @"-target:E:\workplace\NUFL\bin\Debug\MockAssembly\NUFL.TestTarget.dll",
                 @"-targetdir:E:\workplace\NUFL\bin\Debug\MockAssembly",
                 "-targetargs:",
                 "-register:user",
                 "-filter:+[NUFL*]*"
             });
             
            /*
            _commandline = new CommandLineParser(new string[]{
                 "-target:E:\\workplace\\nunit_test\\nunit_test\\bin\\Debug\\nunit_test.exe",
                 "-targetargs:",
                 "-targetdir:E:\\workplace\\nunit_test\\nunit_test\\bin\\Debug\\",
                 "-register:user",
                 //"-coverbytest:",
                 "-filter:+[nunit_test]*"
             });
             */
            _commandline.ExtractAndValidateArguments();
            _filter = Filter.BuildFilter(_commandline);
            _logger = LogManager.GetLogger("test");
            _test_runner = new ProfileTestRunner(_commandline, _filter, _logger);
            IPersistance fault_locator = new FaultLocator();
            _test_runner.Initialize(fault_locator);
  
           
        }
        [Test]
        public void ProfileTestRunnerSmoke()
        {
            _test_runner.Run();
           
        }

        [TearDown]
        public void TearDown()
        {
            _test_runner.Dispose();
        }
    }
}
