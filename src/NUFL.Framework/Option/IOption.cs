﻿//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using System.Collections.Generic;
using NUFL.Framework.Symbol;
using NUFL.Framework.Model;
using NUFL.Framework.ProfilerCommunication;
namespace NUFL.Framework.Setting
{
    /// <summary>
    /// properties exposed by the command line object for use in other entities
    /// </summary>
    public interface IOption
    {
        Registration Registration { get; }
        /// <summary>
        /// the target directory
        /// </summary>
        string TargetDir { get; }

        /// <summary>
        /// If specified then results to be merged by matching hash 
        /// </summary>
        bool MergeByHash { get; }

        /// <summary>
        /// Show the unvisited classes/methods at the end of the coverage run
        /// </summary>
        bool ShowUnvisited { get; }

        /// <summary>
        /// Hide skipped methods from the report
        /// </summary>
        List<SkippedMethod> HideSkipped { get; }

        /// <summary>
        /// Set the threshold i.e. max visit count reporting
        /// </summary>
        ulong Threshold { get; }

        /// <summary>
        /// Set when tracing coverage by test has been enabled
        /// </summary>
        bool TraceByTest { get; }

        /// <summary>
        /// Should auto implemented properties be skipped
        /// </summary>
        bool SkipAutoImplementedProperties { get; }

        /// <summary>
        /// Assemblies containing tests.
        /// </summary>
        IList<string> TestAssemblies { get; }
    }
}