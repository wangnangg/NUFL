using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NUFL.Service;
using System.Threading;
using System.IO;
namespace NUFL.Framework.TestRunner
{
    public class ProcessRunner:INUFLTestRunner
    {
        INUFLTestRunner _remote_runner = null;
        string _key;
        public ProcessRunner(bool is_x64, IEnumerable<Tuple<string, string>> environment, Action<string, string> custom_launch = null)
        {
            _key = Guid.NewGuid().ToString();
            StartRemoteRunnerProcess(is_x64, _key, environment, custom_launch);
        }

        string GetFullPath(string name)
        {
            var directory = new FileInfo(this.GetType().Assembly.Location).DirectoryName;
            return Path.Combine(directory, name);
        }
        string agent_name = "NUFL.Agent.exe";
        string x86agent_name = "NUFL.Agent.x86.exe";
        void StartRemoteRunnerProcess(bool is_x64, string key, IEnumerable<Tuple<string, string>> environment, Action<string, string> custom_launch)
        {
            ProcessStartInfo start_info = new ProcessStartInfo();
            string agent_path;
            if(is_x64)
            {
                agent_path = GetFullPath(agent_name);
            } else
            {
                agent_path = GetFullPath(x86agent_name);
            }
            
            string arguments = key + " " + Process.GetCurrentProcess().Id;
            if (custom_launch != null)
            {
                custom_launch(agent_path, arguments);
            } else
            {
                start_info.FileName = agent_path;
                start_info.Arguments = arguments;
                start_info.UseShellExecute = false;
                if (environment != null)
                {
                    foreach (var pair in environment)
                    {
                        start_info.EnvironmentVariables.Add(pair.Item1, pair.Item2);
                    }
                }
                Process proc = new Process();
                proc.StartInfo = start_info;
                proc.Start();
            }
            
        }

        public void Load(IEnumerable<string> assemblies)
        {
            if(_remote_runner == null)
            {
                int max_time = 100000;
                int gap = 100;
                for (int time = 0; time < max_time; time += gap)
                {
                    try
                    {
                        _remote_runner = (INUFLTestRunner)ServiceManager.Instance.GetService(typeof(INUFLTestRunner), _key);
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        System.Threading.Thread.Sleep(gap);
                    }
                }
            }
            if(_remote_runner == null)
            {
                throw new Exception("Cannot connect to remote runner.");
            }
            try
            {
                _remote_runner.Load(assemblies);
            }
            catch (Exception)
            {
            }
           
        }

        public void RunTests(IEnumerable<string> full_qualified_names, INUFLTestEventListener listener)
        {
            try
            {
                _remote_runner.RunTests(full_qualified_names, listener);
            }
            catch (Exception) { }
        }

        public void StopRun()
        {
            try
            {
                _remote_runner.StopRun();
            }
            catch (Exception)
            {
            }
            
        }

        public List<TestModel.TestCase> DiscoverTests()
        {
            List<TestModel.TestCase> results = new List<TestModel.TestCase>();
            try
            {
                results = _remote_runner.DiscoverTests();
            }
            catch (Exception)
            {
            }
            return results;
            
        }

        public void Dispose()
        {
            try
            {
                _remote_runner.Dispose();
            }
            catch (Exception)
            {
            }
            
        }
    }
}
