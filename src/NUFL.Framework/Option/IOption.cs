//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using System.Collections.Generic;
using NUFL.Framework.Symbol;
using NUFL.Framework.Model;
using NUFL.Framework.ProfilerCommunication;
using System;
using NUFL.Service;
namespace NUFL.Framework.Setting
{
    /// <summary>
    /// properties exposed by the command line object for use in other entities
    /// </summary>
    public interface IOption
    {
        /// <summary>
        /// the target directory
        /// </summary>
        string TargetDir { get; }

        List<string> ProfileFilters { get; }

        List<string> TestAssemblies { get; }

        bool SkipAutoImplementedProperties { get; }

        List<string> Filters { get; }

        string FLMethod { get; }

    }

    public class NUFLOption : GlobalInstanceServiceBase, IOption
    {
        public NUFLOption()
        {
            TestAssemblies = new List<string>();
            ProfileFilters = new List<string>();
        }
        public List<string> TestAssemblies
        {
            set;
            get;
        }

        public string TargetDir
        {
            set;
            get;
        }

        public List<string> ProfileFilters
        {
            set;
            get;
        }


        public string FLMethod { set; get; }

        public bool SkipAutoImplementedProperties
        {
            get { return true; }
        }


        public List<string> Filters
        {
            set;
            get;
        }
    }
}