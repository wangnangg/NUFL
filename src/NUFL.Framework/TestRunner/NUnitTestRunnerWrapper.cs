using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Model;
using NUFL.Framework.Setting;
using NUFL.Framework.TestModel;
using NUnit.Engine.Internal;
using NUnit.Engine.Services;
using NUnit.Engine;
using System.Xml;
namespace NUFL.Framework.TestRunner
{
    public class NUnitTestRunnerWrapper:ITestEventListener, INUFLTestRunner
    {
        TestEngine _engine;
        ITestRunner _runner;
        List<string> _pdb_directories;
        public NUnitTestRunnerWrapper()
        {
            TestEngine engine = new TestEngine();

            var settingsService = new SettingsService(false);
            engine.Services.Add(settingsService);
            engine.Services.Add(new ProjectService());
            engine.Services.Add(new DomainManager());
            engine.Services.Add(new InProcessTestRunnerFactory());
            engine.Services.Add(new DriverService());

            engine.Initialize();

            _engine = engine;
        }
        public void Load(IEnumerable<string> assemblies)
        {
            TestPackage package = new TestPackage(new List<string>(assemblies));
            _pdb_directories = GetPDBDirectories(assemblies);
            _runner = _engine.GetRunner(package);
            _runner.Load();
        }


        public List<TestCase> DiscoverTests()
        {
            List<TestCase> test_cases = TestConverters.ConvertFromNUnitTestCases(_runner.Explore(TestFilter.Empty));
            Program program = new Program(new ProgramEntityFilter(), _pdb_directories);
            foreach (var tc in test_cases)
            {
                program.AddModule(tc.AssemblyPath, "");
                SourceFile file;
                int? line;
                program.FindMethodSourcePosition(tc.AssemblyPath, tc.ClassName, tc.MethodName, out file, out line);
                if (file != null && line != null)
                {
                    tc.CodeFilePath = file.FullName;
                    tc.LineNumber = line.Value;
                }
            }
            return test_cases;
        }

        private List<string> GetPDBDirectories(IEnumerable<string> assemblies)
        {
            List<string> paths = new List<string>();
            foreach (var ass in assemblies)
            {
                paths.Add(new System.IO.FileInfo(ass).DirectoryName);
            }
            List<string> distinct_paths = new List<string>(paths.Distinct<string>());
            return distinct_paths;
        }

        public void Dispose()
        {
            if (_runner != null)
            {
                _runner.Unload();
                _runner.Dispose();
            }
            _engine.Dispose();
        }


        INUFLTestEventListener _listener;

        public void RunTests(IEnumerable<string> full_qualified_names, INUFLTestEventListener listener)
        {
            TestFilter filter = NUnitFilterFactory.CreateNameFilter(full_qualified_names);
            _listener = listener;
            _runner.Run(this, filter);
        }


        public void StopRun()
        {
            _runner.StopRun(true);
        }


        public void OnTestEvent(string report)
        {
            var node = XmlHelper.CreateXmlNode(report);
            switch (node.Name)
            {
                case "start-test":
                    TestStarted(node);
                    break;

                case "test-case":
                    TestFinished(node);
                    break;
            }

        }

        private void TestFinished(XmlNode node)
        {
            try
            {
                TestResult result = TestConverters.ConvertFromNUnitTestResult(node);
                if (_listener != null)
                {
                    _listener.OnTestResult(result);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        private void TestStarted(XmlNode node)
        {
            try
            {
                string fullname = TestConverters.ConvertFromNUnitTestCaseStart(node);
                if (_listener != null)
                {
                    _listener.OnTestStart(fullname);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
    }
}
