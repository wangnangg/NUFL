//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
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
		internal static bool OpenFile(string file)
		{
			try
			{
				if (System.IO.File.Exists(file))
				{
					DTE.ItemOperations.OpenFile(file);
                    return true;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
            return false;
		}

        internal static void NavigateTo(string file, int line)
        {
            if(OpenFile(file))
            {
                GoToLine(line);
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


        /// <summary>
        /// Returns the document file name of the text view.
        /// </summary>
        /// <param name="view">The view instance.</param>
        /// <returns></returns>
        internal static string GetFileName(ITextView view)
        {
            ITextBuffer TextBuffer = view.TextBuffer;

            ITextDocument TextDocument = GetTextDocument(TextBuffer);

            if (TextDocument == null || TextDocument.FilePath == null || TextDocument.FilePath.Equals("Temp.txt"))
            {
                return null;
            }

            return TextDocument.FilePath;
        }

        /// <summary>
        /// Retrives the ITextDocument from the text buffer.
        /// </summary>
        /// <param name="TextBuffer">The text buffer instance.</param>
        /// <returns></returns>
        private static ITextDocument GetTextDocument(ITextBuffer TextBuffer)
        {
            if (TextBuffer == null)
                return null;

            ITextDocument textDoc;
            var rc = TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDoc);

            if (rc == true)
                return textDoc;
            else
                return null;
        }

        /// Given an IWpfTextViewHost representing the currently selected editor pane,
        /// return the ITextDocument for that view. That's useful for learning things 
        /// like the filename of the document, its creation date, and so on.
        internal static ITextDocument GetTextDocumentForView(IWpfTextViewHost viewHost)
        {
            ITextDocument document;
            viewHost.TextView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument), out document);
            return document;
        }

	}
}
