using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using NUFL.GUI.View;
using NUFL.GUI.ViewModel;
using NUFL.GUI.Model;
using NUFL.Service;
using NUFL.Framework.Analysis;
namespace Buaa.NUFL_VSPackage
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("4b3135af-cd3a-48f7-b8d5-e55f11501098")]
    public class MyToolWindow : ToolWindowPane
    {
        FLResultViewModel view_model = new FLResultViewModel();
        FLResultPresenter presenter = new FLResultPresenter();
        EnvDTE.SolutionEvents solution_events;
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public MyToolWindow() :
            base(null)
        {
            // Set the window title reading it from the resources.
            this.Caption = Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            FLResultView view = new FLResultView();
            view.DataContext = view_model;
            base.Content = view;
            presenter.ViewModel = view_model;

            StartNUFLService();
        }

        string solution_key;


        private void StartNUFLService()
        {
            solution_events = NUFL_VSPackagePackage.solution_events;
            solution_events.Opened += OnSolutionOpened;
            solution_events.BeforeClosing += OnSolutionClosing;
            solution_key = Helpers.IDEHelper.GetCurrentSolutionPath();
            if (solution_key != "")
            {
                OnSolutionOpened();
            }

        }

        private void OnSolutionClosing()
        {
            ServiceManager.Instance.UnregisterGlobalService(typeof(IFLResultPresenter), solution_key);
        }


        private void OnSolutionOpened()
        {
            solution_key = GetSolutionKey();
            ServiceManager.Instance.RegisterGlobalInstanceService(typeof(IFLResultPresenter), presenter, solution_key);
        }

        private string GetSolutionKey()
        {
            var dte = NUFL_VSPackagePackage.DTE;
            return new System.IO.FileInfo(Helpers.IDEHelper.GetCurrentSolutionPath()).Directory.FullName + "\\";
        }

    }
}
