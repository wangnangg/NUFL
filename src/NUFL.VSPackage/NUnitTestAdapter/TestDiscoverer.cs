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
using NUnit.Engine;
using NUFL.Server;

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
            var runner_factory = new NUFL.Framework.TestRunner.SimpleRunnerFactory();
            using (var runner = runner_factory.CreateDiscoverer())
            {
                runner.Load(sources);
                var nufl_test_cases = runner.DiscoverTests();
                foreach (var test_case in nufl_test_cases)
                {
                    TestCase vs_test_case = new TestCase(test_case.FullyQualifiedName, TestExecutor.Uri, test_case.AssemblyPath)
                    {
                        DisplayName = test_case.DisplayName,
                        CodeFilePath = test_case.CodeFilePath,
                        LineNumber = test_case.LineNumber,
                    };
                    discoverySink.SendTestCase(vs_test_case);
                }
                runner.Unload();
            }
        }
    }
}
