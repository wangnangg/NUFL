using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using log4net;
using NUnit.Engine;
using NUnit.Engine.Runners;
using NUnit.Engine.Internal;
using NUFL.Framework.ProfilerCommunication;
using NUFL.Framework.Setting;
using NUnit.Engine.Services;

namespace NUFL.Framework.Test.ProfilerCommunication
{
    [TestFixture]
    class ProfilerMessageDispatcherTests
    {
        NUFLSetting _option;
        IFilter _filter;
        ILog _logger;
        [SetUp]
        public void Setup()
        {

            _option = new NUFLSetting();
            _logger = LogManager.GetLogger("test");
        }
        [Test]
        public void MessageArrivalTest()
        {
            UInt32 num;
            ProfilerMessageDispatcher dispatcher = new ProfilerMessageDispatcher();
            dispatcher.RegisterHandler(MSG_Type.MSG_TrackAssembly, new Action<object, IPCStream>(
                (msg, data_stream) =>
                {
                    MSG_TrackAssembly_Request ta_req = (MSG_TrackAssembly_Request)msg;
                    byte[] tmp = new byte[4];
                    data_stream.Read(tmp, 0, 4);
                    num = BitConverter.ToUInt32(tmp, 0);
                    Console.WriteLine("{0}\n{1}", ta_req.modulePath, ta_req.assemblyName);
                    Console.WriteLine("number is {0}", num);
                }));
            dispatcher.Start();
            IPCStream client_stream = new IPCStream(dispatcher.MsgStreamGuid, dispatcher.MsgStreamBufferSize);
            MSG_TrackAssembly_Request client_req;
            client_req.assemblyName = "my aseembly name.";
            client_req.modulePath = "my assembly path.";
            client_req.type = MSG_Type.MSG_TrackAssembly;
            byte[] buffer = StrToBytes<MSG_TrackAssembly_Request>(client_req);
            client_stream.Write(buffer, 0, (UInt32)buffer.Length);
            client_stream.Write(BitConverter.GetBytes((UInt32)42), 0, 4);
            client_stream.Flush();
            dispatcher.Stop();
        }

        static byte[] StrToBytes<T>(T str)
        {
            int size = Marshal.SizeOf(str);
            byte[] buffer = new byte[ProfilerMessageDispatcher.MsgMaxSize];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, buffer, 0, size);
            Marshal.FreeHGlobal(ptr);
            return buffer;
        }

        [Test]
        public void CommunicationWithProfiler()
        {
            ProfilerMessageDispatcher dispatcher = new ProfilerMessageDispatcher();
            dispatcher.RegisterHandler(MSG_Type.MSG_TrackAssembly, new Action<object, IPCStream>(
                (msg, data_stream) =>
                {
                    MSG_TrackAssembly_Request ta_req = (MSG_TrackAssembly_Request)msg;
                    Debug.WriteLine("message Track Assembly received");
                    MSG_TrackAssembly_Response response;
                    response.track = false;
                    response.testInvokerCount = 5;
                    response.invokerTokens = new int[MSG_TrackAssembly_Response.TOKENS_SIZE_CONST];
                    for (int i = 0; i < response.testInvokerCount; i++)
                    {
                        response.invokerTokens[i] = i + 1000;
                    }
                    ProfilerMessageDispatcher.SendMessage<MSG_TrackAssembly_Response>(data_stream, ref response);
                }));
            dispatcher.Start();
            RunProfiler(@"E:\workplace\nunit_test\nunit_test\bin\Debug\nunit_test.exe", dispatcher);
            dispatcher.Stop();
            dispatcher.Dispose();
        }

        void RunProfiler(string target, ProfilerMessageDispatcher dispatcher)
        {
            ProcessStartInfo startinfo = new ProcessStartInfo(target);
            var dictionary = startinfo.EnvironmentVariables;
            dictionary[@"OpenCover_Profiler_Key"] = dispatcher.MsgStreamGuid;
            dictionary[@"OpenCover_Profiler_Namespace"] = "Local";
            dictionary[@"OpenCover_Profiler_Threshold"] = "1";
            dictionary["Cor_Profiler"] = "{9E0614F2-BE35-4A96-A56D-25C59F3684E2}";
            dictionary["Cor_Enable_Profiling"] = "1";
            dictionary["CoreClr_Profiler"] = "{9E0614F2-BE35-4A96-A56D-25C59F3684E2}";
            dictionary["CoreClr_Enable_Profiling"] = "1";
            dictionary["Cor_Profiler_Path"] = ProfilerRegistration.GetProfilerPath(false);
            dictionary["OpenCover_Msg_Buffer_Guid"] = dispatcher.MsgStreamGuid;
            dictionary["OpenCover_Msg_Buffer_Size"] = dispatcher.MsgStreamBufferSize.ToString();
            dictionary["OpenCover_Data_Buffer_Guid"] = dispatcher.DataStream.UniqueGuid;
            dictionary["OpenCover_Data_Buffer_Size"] = dispatcher.DataStream.BufferSize.ToString();
            startinfo.UseShellExecute = false;

            Process p = new Process();
            p.StartInfo = startinfo;
            p.Start();
            //  EventWaitHandle debugging_event = new EventWaitHandle(false, EventResetMode.AutoReset, "wangnan_debugging_event");
            // debugging_event.Set();
            p.WaitForExit();

        }

        void RunNunit(string target, ProfilerMessageDispatcher dispatcher)
        {
            ITestEngine engine = new TestEngine();
            TestPackage package = new TestPackage(target);
            package.Settings.Add("ShadowCopyFiles", true);
            TestFilter filter = new TestFilter("<filter></filter>");
            ServiceContext sc = (ServiceContext)engine.Services;
            var eventHandler = new TestEventHandler();

            ITestAgent agent = sc.GetService<TestAgency>().GetAgent(
                package,
                30000,
                (dictionary) =>
                {
                    dictionary[@"OpenCover_Profiler_Key"] = dispatcher.MsgStreamGuid;
                    dictionary[@"OpenCover_Profiler_Namespace"] = "Local";
                    dictionary[@"OpenCover_Profiler_Threshold"] = "1";
                    dictionary["Cor_Profiler"] = "{9E0614F2-BE35-4A96-A56D-25C59F3684E2}";
                    dictionary["Cor_Enable_Profiling"] = "1";
                    dictionary["CoreClr_Profiler"] = "{9E0614F2-BE35-4A96-A56D-25C59F3684E2}";
                    dictionary["CoreClr_Enable_Profiling"] = "1";
                    dictionary["Cor_Profiler_Path"] = ProfilerRegistration.GetProfilerPath(false);
                    dictionary["OpenCover_Msg_Buffer_Guid"] = dispatcher.MsgStreamGuid;
                    dictionary["OpenCover_Msg_Buffer_Size"] = dispatcher.MsgStreamBufferSize.ToString();
                    dictionary["OpenCover_Data_Buffer_Guid"] = dispatcher.DataStream.UniqueGuid;
                    dictionary["OpenCover_Data_Buffer_Size"] = dispatcher.DataStream.BufferSize.ToString();
                });
            ITestEngineRunner remote_runner = agent.CreateRunner(package);
            remote_runner.Load();
            var result = remote_runner.Run(eventHandler, filter).Aggregate("test-run", package.Name, package.FullName).Xml; ;
            remote_runner.Unload();
            remote_runner.Dispose();
            // agent.Stop();
            agent.Stop();

        }

        void ProcessCovData(object arg)
        {


        }

        [Test]
        public void NunitCommunicationWithProfiler()
        {
            ProfilerMessageDispatcher dispatcher = new ProfilerMessageDispatcher();
            dispatcher.RegisterHandler(MSG_Type.MSG_TrackAssembly,
                (msg, data_stream) =>
                {
                    MSG_TrackAssembly_Request ta_req = (MSG_TrackAssembly_Request)msg;
                    Debug.WriteLine("message Track Assembly received");
                    MSG_TrackAssembly_Response response;
                    response.track = false;
                    response.testInvokerCount = 0;
                    response.invokerTokens = new int[MSG_TrackAssembly_Response.TOKENS_SIZE_CONST];
                    ProfilerMessageDispatcher.SendMessage<MSG_TrackAssembly_Response>(data_stream, ref response);
                });
            dispatcher.Start();
            RunNunit(@"E:\workplace\NUFL\bin\Debug\NUFL.TestTarget.dll", dispatcher);
            dispatcher.Stop();
            dispatcher.Dispose();
        }


        public class TestEventHandler : MarshalByRefObject, ITestEventListener
        {

            public void OnTestEvent(string report)
            {
                Debug.WriteLine("onTestEvent" + report);
            }
        }
    }
}
