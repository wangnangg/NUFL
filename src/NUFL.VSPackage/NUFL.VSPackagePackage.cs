using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using NUFL.Framework.TestRunner;
using NUFL.Service;
using NUFL.Framework.Setting;
using NUFL.Framework.Analysis;
using NUFL.Framework.Persistance;
using System.Collections.Generic;
using Buaa.NUFL_VSPackage.View;
namespace Buaa.NUFL_VSPackage
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(MyToolWindow))]
    [Guid(GuidList.guidNUFL_VSPackagePkgString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class NUFL_VSPackagePackage : Package
    {
        static EnvDTE.DTE DTE { set; get; }
        static EnvDTE.SolutionEvents SolutionEvents { set; get; }
        static string _solution_key = "";
        NUFLSetting Option = new NUFLSetting();
        public static string SolutionKey 
        {
            get
            {
                return _solution_key;
            }
            set
            {
                if(_solution_key == value)
                {
                    return;
                }
                if(value == "")
                {
                    if (SolultionKeyCleared != null)
                    {
                        SolultionKeyCleared(_solution_key);
                    }
                    _solution_key = value;
                } else
                {
                    _solution_key = value;
                    if (SolutionKeySet != null)
                    {
                        SolutionKeySet(_solution_key);
                    }
                }
                
                
            }
        }
        public static event Action<string> SolutionKeySet;
        public static event Action<string> SolultionKeyCleared;
        RemoteRunnerFactory _runner_factory;
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public NUFL_VSPackagePackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            _runner_factory = new RemoteRunnerFactory()
            {
                Option = Option
            };
            ServiceManager.Instance.RegisterLocalService(typeof(ISetting), Option);
            SolutionKeySet += (key) => 
                {
                    ServiceManager.Instance.RegisterGlobalService(typeof(IRunnerFactory), _runner_factory, key);
                };
            SolultionKeyCleared += (key) =>
                {
                    ServiceManager.Instance.UnregisterGlobalService(typeof(IRunnerFactory), key);
                };
        }


        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

 


        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            DTE = GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            SolutionEvents = DTE.Events.SolutionEvents;
            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidNUFL_VSPackageCmdSet, (int)PkgCmdIDList.cmdidResult);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );
            }


            //start service
            StartNUFLService();
        }


        private void StartNUFLService()
        {

            SolutionEvents.Opened += OnSolutionOpened;
            SolutionEvents.BeforeClosing += OnSolutionClosing;
            SolutionKey = GetSolutionKey();
            if (SolutionKey != "")
            {
                OnSolutionOpened();
            }
            
        }

        private void OnSolutionClosing()
        {
            Option.SetBackup("", "");
            SolutionKey = "";

        }


        private void OnSolutionOpened()
        {
            string solution_config_name = Helpers.IDEHelper.GetCurrentSolutionPath() + ".nufl.config";
            Option.SetBackup(solution_config_name, "");
            SolutionKey = GetSolutionKey();
        }

        private string GetSolutionKey()
        {
            string solution_path = Helpers.IDEHelper.GetCurrentSolutionPath();
            if (solution_path == "")
            {
                return "";
            }

            return new System.IO.FileInfo(solution_path).Directory.FullName + "\\";
        }
        #endregion

    }
}
