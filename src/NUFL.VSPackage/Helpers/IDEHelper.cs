//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VSLangProj;

namespace Buaa.NUFL_VSPackage.Helpers
{
	internal static class IDEHelper
	{
		public static EnvDTE.DTE DTE;

		/// <summary>
		/// Initializes the <see cref="IDEHelper"/> class.
		/// </summary>
		static IDEHelper()
		{
			DTE = (Package.GetGlobalService(typeof(EnvDTE.DTE))) as EnvDTE.DTE;
		}

		/// <summary>
		/// Opens the file in Visual Studio.
		/// </summary>
		/// <param name="file">The file path.</param>
		internal static void OpenFile(string file)
		{
			try
			{
				if (System.IO.File.Exists(file))
				{
					DTE.ItemOperations.OpenFile(file);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

        internal static string GetCurrentSolutionPath()
        {
            return DTE.Solution.FullName;
        }

		/// <summary>
		/// Closes the file.
		/// </summary>
		/// <param name="DTE">The DTE.</param>
		/// <param name="fileName">Name of the file.</param>
		internal static void CloseFile(string fileName)
		{
			foreach (EnvDTE.Document document in DTE.Documents)
			{
				if (fileName.Equals(document.FullName, StringComparison.InvariantCultureIgnoreCase))
				{
					document.Close();
					break;
				}
			}
		}

		/// <summary>
		/// Moves the caret to line number.
		/// </summary>
		/// <param name="DTE">The DTE.</param>
		/// <param name="lineNumber">The line number.</param>
		internal static void GoToLine(int lineNumber)
		{
			DTE.ExecuteCommand("GotoLn", lineNumber.ToString());
		}

       
	}
}
