using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NUFL.Framework.Setting;
using NUFL.Framework.Persistance;
using log4net;
using NUFL.Framework.ProfilerCommunication;
using INUnitTestEventListener = NUnit.Engine.ITestEventListener;
using NUnit.Engine;
using NUnit.Engine.Internal;
using NUFL.Framework.Model;
using System.Threading;
using NUFL.Framework.Symbol;
using NUFL.Framework.TestModel;
using NUFL.Service;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Remoting.Lifetime;
using NUnit.Engine.Services;
namespace NUFL.Framework.TestRunner
{
    class ProfileTestRunner : RemoteRunnerBase, INUnitTestEventListener, INUFLTestRunner
    {
        ProfilerMessageDispatcher _profiler_msg_dispatcher;
        Program _program;
        Thread _data_process_thread;
        public ISetting Option
        {
            get;
            set;
        }
        public IPersistance ProfilePersistance
        {
            get;
            set;
        }

        ITestEngineRunner _runner;
        ITestEngine _engine;
        ITestAgent _agent;
        TestAgency _agency;
        List<string> _pdb_directories;
        public ProfileTestRunner()
        {
            _engine = new TestEngine();
        }
        public void Load(IEnumerable<string> assemblies)
        {
            _pdb_directories = GetPDBDirectories(assemblies);
            _program = new Program(Option.GetSetting<ProgramEntityFilter>("filter"), _pdb_directories);
            ProfilePersistance.PersistProgram(_program);
            StartProfilerServer();
            TestPackage package = new TestPackage(new List<string>(assemblies));
            _runner = CreateRunner(package);
            _runner.Load();
        }





        private List<string> GetPDBDirectories(IEnumerable<string> assemblies)
        {
            List<string> paths = new List<string>();
            foreach(var ass in assemblies)
            {
                paths.Add(new System.IO.FileInfo(ass).DirectoryName);
            }
            List<string> distinct_paths = new List<string>(paths.Distinct<string>());
            return distinct_paths;
        }

        private void StartProfilerServer()
        {
            //cache module
            _profiler_msg_dispatcher = new ProfilerMessageDispatcher();
            _profiler_msg_dispatcher.RegisterHandler(MSG_Type.MSG_TrackAssembly, TrackAssemblyHandler);
            _profiler_msg_dispatcher.RegisterHandler(MSG_Type.MSG_GetSequencePoints, GetSequencePointsHandler);
            _profiler_msg_dispatcher.Start();
            ThreadStart ts = new ThreadStart(ProcessCovData);
            _data_process_thread = new Thread(ts);
            _data_process_thread.Start();
        }

        private ITestEngineRunner CreateRunner(TestPackage package)
        {
            ServiceContext sc = (ServiceContext)_engine.Services;
            Action<System.Collections.Specialized.StringDictionary> environment;
            environment = (dictionary) =>
            {
                dictionary[@"OpenCover_Profiler_Key"] = _profiler_msg_dispatcher.MsgStreamGuid;
                dictionary[@"OpenCover_Profiler_Namespace"] = "Local";
                dictionary[@"OpenCover_Profiler_Threshold"] = "1";
                dictionary["Cor_Profiler"] = "{9E0614F2-BE35-4A96-A56D-25C59F3684E2}";
                dictionary["Cor_Enable_Profiling"] = "1";
                dictionary["CoreClr_Profiler"] = "{9E0614F2-BE35-4A96-A56D-25C59F3684E2}";
                dictionary["CoreClr_Enable_Profiling"] = "1";
                dictionary["Cor_Profiler_Path"] = ProfilerRegistration.GetProfilerPath(false);
                dictionary["OpenCover_Msg_Buffer_Guid"] = _profiler_msg_dispatcher.MsgStreamGuid;
                dictionary["OpenCover_Msg_Buffer_Size"] = _profiler_msg_dispatcher.MsgStreamBufferSize.ToString();
                dictionary["OpenCover_Data_Buffer_Guid"] = _profiler_msg_dispatcher.DataStream.UniqueGuid;
                dictionary["OpenCover_Data_Buffer_Size"] = _profiler_msg_dispatcher.DataStream.BufferSize.ToString();
                dictionary["OpenCover_Profiler_TraceByTest"] = "1";
            };
            _agency = sc.GetService<TestAgency>();
            _agent = _agency.GetAgent(
                package,
                30000,
                environment);
            ITestEngineRunner remote_runner = _agent.CreateRunner(package);
            return remote_runner;

        }


        bool _unloaded = false;
        object _lock = new object();
        public void Unload()
        {
            lock (_lock)
            {
                if (_unloaded)
                {
                    return;
                }
                try
                {
                    _runner.Unload();
                }
                catch (Exception)
                {

                }
                //_runner.Dispose();
                try
                {
                    _agent.Stop();
                }
                catch (Exception)
                {

                }
                // _agency.ReleaseAgent(_agent);

                // }catch(Exception e)
                //  {
                //     Debug.WriteLine("error trying to unload runner. " + e.Message);
                // }

                //stop sever
                _profiler_msg_dispatcher.Stop();
                _profiler_msg_dispatcher.UnregisterHandler(MSG_Type.MSG_TrackAssembly, TrackAssemblyHandler);
                _profiler_msg_dispatcher.UnregisterHandler(MSG_Type.MSG_GetSequencePoints, GetSequencePointsHandler);
                _data_process_thread.Join();
                _profiler_msg_dispatcher.Dispose();

                ProfilePersistance.Commit();

                _unloaded = true;
            }
        }

        INUFLTestEventListener _listener = null;

        public void RunTests(IEnumerable<string> full_qualified_names, INUFLTestEventListener listener)
        {
            NameFilter filter = new NameFilter(full_qualified_names);
            RunTests(filter, listener);
        }

        public void RunAllTests(INUFLTestEventListener listener)
        {
            TestFilter filter = TestFilter.Empty;
            RunTests(filter, listener);
        }

        void RunTests(TestFilter filter, INUFLTestEventListener listener)
        {
            _listener = listener;
            try
            {
                _runner.Run(this, filter);
            }catch(Exception)
            {
                Unload();
            }

        }
        public void StopRun()
        {
            try
            {
                _runner.StopRun(true);
            }catch(Exception)
            {
                Unload();
            }
        }

        public List<TestCase> DiscoverTests()
        {
            List<TestCase> test_cases = TestConverters.ConvertFromNUnitTestCases(_runner.Explore(TestFilter.Empty).Xml);
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

        private void ProcessCovData()
        {
            IPCStream data_stream = _profiler_msg_dispatcher.DataStream;
            UInt32 read_size = data_stream.BufferSize / 4 * 4;
            byte[] data = new byte[read_size];
            UInt32[] cov_data = new UInt32[read_size / 4];

            UInt32 actual_read_size = data_stream.ReadOnce(data, 0, read_size);
            while (actual_read_size > 0)
            {
                //process coverage data
                UInt32 cov_data_size = actual_read_size / 4;

                for (int i = 0, offset = 0; i < cov_data_size; i += 1, offset += 4)
                {
                    cov_data[i] = BitConverter.ToUInt32(data, offset); 
                }

                ProfilePersistance.SaveCoverageData(cov_data, cov_data_size);

                //deal with remainant
                UInt32 remain_offset = cov_data_size * 4;
                UInt32 remain_size = actual_read_size - remain_offset;
                for (int i = 0; i < remain_size; i++ )
                {
                    data[i] = data[remain_offset + i];
                }
                actual_read_size = data_stream.ReadOnce(data, remain_size, read_size - remain_size);
                
            }
        }

        private void TrackAssemblyHandler(object msg_obj, IPCStream msg_stream)
        {
            MSG_TrackAssembly_Request ta_req = (MSG_TrackAssembly_Request)msg_obj;
            MSG_TrackAssembly_Response response;
            //Debug.WriteLine("track " + ta_req.assemblyName);

            var module = _program.AddModule(ta_req.modulePath, ta_req.assemblyName);

            response.track = module.Skipped ? false : true;

            //inspect test invokers
            var tokens = NUnitTestInvokerFinder.GetInvokerTokens(ta_req.modulePath, ta_req.assemblyName);
            response.testInvokerCount = tokens.Count;
            response.invokerTokens = new int[MSG_TrackAssembly_Response.TOKENS_SIZE_CONST];
            for (int i = 0; i < tokens.Count; i++ )
            {
                response.invokerTokens[i] = tokens[i];
            }
            ProfilerMessageDispatcher.SendMessage<MSG_TrackAssembly_Response>(msg_stream, ref response);
        }

        private void GetSequencePointsHandler(object msg_obj, IPCStream msg_stream)
        {
            MSG_GetSequencePoints_Request gsp_req = (MSG_GetSequencePoints_Request)msg_obj;
            var points = _program.GetSequencePointsForMethod(gsp_req.modulePath, gsp_req.functionToken);

            MSG_GetSequencePoints_Response gsp_resp = new MSG_GetSequencePoints_Response();
            gsp_resp.count = points.Count;
            ProfilerMessageDispatcher.SendMessageNoFlush<MSG_GetSequencePoints_Response>(msg_stream, ref gsp_resp);
            foreach (var point in points)
            {
                MSG_SequencePoint sp;
                sp.offset = point.Offset;
                sp.uniqueId = point.UniqueSequencePoint;
                byte[] bytes = ProfilerMessageDispatcher.ToBytes<MSG_SequencePoint>(ref sp);
                msg_stream.Write(bytes, 0, (UInt32)bytes.Length);
            }
            msg_stream.Flush();
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
                ProfilePersistance.PersistTestResult(result);
                if (_listener != null)
                {
                    _listener.OnTestResult(result);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
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
                Debug.WriteLine(e.Message);
            }
        }

        public override void Dispose()
        {
            _engine.Dispose();
            base.Dispose();
        }



    }

}
