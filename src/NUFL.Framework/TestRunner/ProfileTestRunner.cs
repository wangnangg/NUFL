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
using NUnit.Engine;
using NUnit.Engine.Internal;
using NUFL.Framework.Model;

namespace NUFL.Framework.TestRunner
{
    public class ProfileTestRunner : MarshalByRefObject, IDisposable, ITestEventListener
    {
        IOption _option;
        IFilter _filter;
        ILog _logger;
        IPersistance _profile_persistance;
        ProfilerMessageDispatcher _profiler_msg_dispatcher;
        IInstrumentationModelBuilderFactory _builder_factory;
        ModuleCache _module_cache;

        public ProfileTestRunner(IOption option, IFilter filter, ILog logger)
        {
            _option = option;
            _filter = filter;
            _logger = logger;
        }

        public void Initialize(IPersistance profile_persistance)
        {
            _profile_persistance = profile_persistance;
            _profiler_msg_dispatcher = new ProfilerMessageDispatcher();
            _builder_factory = new InstrumentationModelBuilderFactory(_option, _filter, _logger);
            _module_cache = new ModuleCache();

            _profiler_msg_dispatcher.RegisterHandler(MSG_Type.MSG_TrackAssembly, TrackAssemblyHandler);
            _profiler_msg_dispatcher.RegisterHandler(MSG_Type.MSG_TrackMethod, TrackMethodHandler);
            _profiler_msg_dispatcher.RegisterHandler(MSG_Type.MSG_GetSequencePoints, GetSequencePointsHandler);
            _profile_persistance.ConnectDataStream(_profiler_msg_dispatcher.DataStream);
            //ProfilerRegistration.Register(_option.Registration);

        }

        public void Run()
        {
            _profiler_msg_dispatcher.Start();
            //run nunit with profiler
            RunNunitWithProfiler();
            _profiler_msg_dispatcher.Stop();
            
        }

        private void RunNunitWithProfiler()
        {
            using (ITestEngine engine = new TestEngine())
            {
                TestPackage package = new TestPackage(_option.TestAssemblies);
                package.Settings.Add("ShadowCopyFiles", true);
                TestFilter filter = TestFilter.Empty;
                ServiceContext sc = (ServiceContext)engine.Services;

                ITestAgent agent = sc.TestAgency.GetAgent(
                    sc.RuntimeFrameworkSelector.SelectRuntimeFramework(package),
                    30000,
                    false,   //debug?
                    "",
                    true, //x86?
                    (dictionary) =>
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
                    });
                using (ITestEngineRunner remote_runner = agent.CreateRunner(package))
                {
                    TestEngineResult result = remote_runner.Load();
                    Debug.WriteLine("Test count {0}", remote_runner.CountTestCases(filter));
                    remote_runner.Run(this, filter);
                    remote_runner.Unload();
                }
                agent.Stop();
                sc.TestAgency.WaitAgent(agent);

            }
        }

        private void TrackAssemblyHandler(object msg_obj, IPCStream msg_stream)
        {
            MSG_TrackAssembly_Request ta_req = (MSG_TrackAssembly_Request)msg_obj;
            MSG_TrackAssembly_Response response;
            Debug.WriteLine("track " + ta_req.assemblyName);
            var module_builder = _builder_factory.CreateModelBuilder(ta_req.modulePath, ta_req.assemblyName);
            if(_filter.UseAssembly(ta_req.assemblyName) && module_builder.CanInstrument)
            {
                var module = module_builder.BuildModuleModel(true);
                //to do: record this module for getsequencepoints
                _module_cache.AddModule(module);
                _profile_persistance.PersistModule(module);
                response.track = true;
            } else
            {
                response.track = false;
            }
            ProfilerMessageDispatcher.SendMessage<MSG_TrackAssembly_Response>(msg_stream, ref response);
        }

        private void TrackMethodHandler(object msg_obj, IPCStream msg_stream)
        {

        }
        private void GetSequencePointsHandler(object msg_obj, IPCStream msg_stream)
        {
            MSG_GetSequencePoints_Request gsp_req = (MSG_GetSequencePoints_Request)msg_obj;
            var method = _module_cache.RetrieveMethod(gsp_req.modulePath, gsp_req.functionToken);
            InstrumentationPoint[] points = method.SequencePoints;
            MSG_GetSequencePoints_Response gsp_resp = new MSG_GetSequencePoints_Response();
            gsp_resp.count = points.Length;
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
            Debug.WriteLine("Sending points from " + method.Name);
        }


        public void Dispose()
        {
            _profiler_msg_dispatcher.Dispose();
          //  ProfilerRegistration.Unregister(Registration.Path32);
        }



        public void OnTestEvent(string report)
        {
            _profile_persistance.PersistTestResult(report);
        }
    }

}
