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
using NUFL.Framework.Analysis;
using NUFL.Framework.Model;
using NUFL.Framework.ProfilerCommunication;
using System.Diagnostics;
using NUFL.GUI.ViewModel;
using NUFL.Service;
using System.Collections.ObjectModel;
namespace NUFL.Framework.Test.Analysis
{
    [TestFixture]
    class FaultLocatorTests
    {
        RemoteRunnerFactory _runner_factory;
        string[] assemblies = new string[] { @".\MockAssembly\NUFL.TestTarget.dll" };
    }
}
