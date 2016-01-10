using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Setting;
using NUFL.Framework.Persistance;
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
    public class ProfileRunner:INUFLTestRunner
    {
        ProfilerMessageDispatcher _profiler_msg_dispatcher;
        Program _program;
        Thread _data_process_thread;
        ISetting _setting;
        IPersistance _persistence;
        bool _is_x64;
        INUFLTestRunner _process_runner;
        List<string> _pdb_directories;
        public ProfileRunner(bool is_x64,ISetting setting, IPersistance persistence)
        {
            _is_x64 = is_x64;
            _setting = setting;
            _persistence = persistence;
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

        public void Load(IEnumerable<string> assemblies)
        {
            GlobalEventManager.Instance.RaiseEvent(new GlobalEvent()
                {
                    Name = EventEnum.StatusChanged,
                    Argument = "Loading tests...",
                    Sender = this

                });
            _pdb_directories = GetPDBDirectories(assemblies);
            _program = new Program(_setting.GetSetting<ProgramEntityFilter>("filter"), _pdb_directories);
            _persistence.PersistProgram(_program);
            StartProfilerServer();
            _process_runner = CreateRunner();
            _process_runner.Load(assemblies);
        }

        private INUFLTestRunner CreateRunner()
        {
            List<Tuple<string, string>> env = new List<Tuple<string, string>>();
            env.Add(new Tuple<string, string>(@"OpenCover_Profiler_Key", _profiler_msg_dispatcher.MsgStreamGuid));
            env.Add(new Tuple<string, string>(@"OpenCover_Profiler_Namespace", "Local"));
            env.Add(new Tuple<string, string>(@"OpenCover_Profiler_Threshold", "1"));
            env.Add(new Tuple<string, string>("Cor_Profiler", "{9E0614F2-BE35-4A96-A56D-25C59F3684E2}"));
            env.Add(new Tuple<string, string>("Cor_Enable_Profiling", "1"));
            env.Add(new Tuple<string, string>("CoreClr_Profiler","{9E0614F2-BE35-4A96-A56D-25C59F3684E2}"));
            env.Add(new Tuple<string, string>("CoreClr_Enable_Profiling", "1"));
            env.Add(new Tuple<string, string>("Cor_Profiler_Path", ProfilerRegistration.GetProfilerPath(_is_x64)));
            env.Add(new Tuple<string, string>("OpenCover_Msg_Buffer_Guid", _profiler_msg_dispatcher.MsgStreamGuid));
            env.Add(new Tuple<string, string>("OpenCover_Msg_Buffer_Size", _profiler_msg_dispatcher.MsgStreamBufferSize.ToString()));
            env.Add(new Tuple<string, string>("OpenCover_Data_Buffer_Guid", _profiler_msg_dispatcher.DataStream.UniqueGuid));
            env.Add(new Tuple<string, string>("OpenCover_Data_Buffer_Size", _profiler_msg_dispatcher.DataStream.BufferSize.ToString()));
            env.Add(new Tuple<string, string>("OpenCover_Profiler_TraceByTest", "1"));
            ProcessRunner runner = new ProcessRunner(_is_x64, env);
            return runner;
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

                _persistence.SaveCoverageData(cov_data, cov_data_size);

                //deal with remainant
                UInt32 remain_offset = cov_data_size * 4;
                UInt32 remain_size = actual_read_size - remain_offset;
                for (int i = 0; i < remain_size; i++)
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
            for (int i = 0; i < tokens.Count; i++)
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

        public void RunTests(IEnumerable<string> full_qualified_names, INUFLTestEventListener listener)
        {
            GlobalEventManager.Instance.RaiseEvent(new GlobalEvent()
            {
                Name = EventEnum.StatusChanged,
                Argument = "Running tests...",
                Sender = this

            });
            _process_runner.RunTests(full_qualified_names, new Listener(listener, _persistence));
        }

        public void StopRun()
        {
            _process_runner.StopRun();
        }

        public List<TestCase> DiscoverTests()
        {
            return _process_runner.DiscoverTests();
        }

        public void Dispose()
        {
            GlobalEventManager.Instance.RaiseEvent(new GlobalEvent()
            {
                Name = EventEnum.StatusChanged,
                Argument = "Preparing results...",
                Sender = this

            });
            
            _profiler_msg_dispatcher.Stop();

            _data_process_thread.Join();
            _persistence.Commit();

            _process_runner.Dispose();
            _profiler_msg_dispatcher.UnregisterHandler(MSG_Type.MSG_TrackAssembly, TrackAssemblyHandler);
            _profiler_msg_dispatcher.UnregisterHandler(MSG_Type.MSG_GetSequencePoints, GetSequencePointsHandler);
            _profiler_msg_dispatcher.Dispose();

            GlobalEventManager.Instance.RaiseEvent(new GlobalEvent()
            {
                Name = EventEnum.StatusChanged,
                Argument = "Ready",
                Sender = this

            });
            
        }

        class Listener:MarshalByRefObject,INUFLTestEventListener
        {
            INUFLTestEventListener _outer_listener;
            IPersistance _persistence;
            public Listener(INUFLTestEventListener outer_listener, IPersistance persistence)
            {
                _outer_listener = outer_listener;
                _persistence = persistence;
            }

            public void OnTestStart(string fullname)
            {
                _outer_listener.OnTestStart(fullname);
            }

            public void OnTestResult(TestResult result)
            {
                _persistence.PersistTestResult(result);
                _outer_listener.OnTestResult(result);
            }
            public override object InitializeLifetimeService()
            {
                return null;
            }
        }


    }
}
