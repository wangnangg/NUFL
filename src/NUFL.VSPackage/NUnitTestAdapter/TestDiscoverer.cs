using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using NUFL.Framework;
using NUFL.Service;
using System.Diagnostics;
namespace Buaa.NUFL_VSPackage.NUnitTestAdapter
{
    [ExtensionUri(UriString)]
    [DefaultExecutorUri(TestExecutor.UriString)]
    public class TestDiscoverer:ITestDiscoverer
    {
        public const string UriString = "executor://NUFL.NUnitTestDiscoverer";
        public static Uri Uri = new Uri(UriString);
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            var runner_factory = new NUFL.Framework.TestRunner.RunnerFactory();
            runner_factory.IsX64 = Environment.Is64BitProcess;
            runner_factory.OwnerPid = Process.GetCurrentProcess().Id;
            using (var runner = runner_factory.GetProcessRunner(null))
            {
                runner.Load(sources);
                var nufl_test_cases = runner.DiscoverTests();
                foreach (var test_case in nufl_test_cases)
                {
                    discoverySink.SendTestCase(Converter.ConvertFromNUFLTestCase(test_case));
                }
            }
        }
    }
}
