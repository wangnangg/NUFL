﻿//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NUFL.Framework.Model
{
    /// <summary>
    /// The details of a module
    /// </summary>
    public class Module:ProgramEntityBase
    {
        /// <summary>
        /// simple constructor
        /// </summary>
        public Module()
        {
            Aliases = new List<string>();
        }

        /// <summary>
        /// The full path name to the module
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// A list of aliases
        /// </summary>
        [XmlIgnore]
        public IList<string> Aliases { get; private set; } 

        /// <summary>
        /// The name of the module
        /// </summary>
        public string ModuleName { get; set; }


        /// <summary>
        /// The classes that make up the module
        /// </summary>
        public Class[] Classes { get; set; }

        public bool Skipped { get; set; }

        /// <summary>
        /// A hash of the file used to group them together (especially when running against mstest)
        /// </summary>
        [XmlAttribute("hash")]
        public string ModuleHash { get; set; }



        public override IEnumerable<ProgramEntityBase> DirectChildren
        {
            get
            {
                foreach (var @class in Classes)
                {
                    if(@class.Skipped || @class.Methods == null || @class.Methods.Length == 0)
                    {
                        continue;
                    }
                    yield return @class;
                }
                yield break;
            }
        }

        protected override List<ProgramEntityBase> GetDirectChildrenSortedByCov()
        {
            List<ProgramEntityBase> children = new List<ProgramEntityBase>(Classes);
            children.Sort((x, y) => { return x.CoveragePercent.CompareTo(y.CoveragePercent); });
            return children;
        }

        protected override List<ProgramEntityBase> GetDirectChildrenSortedBySusp()
        {
            List<ProgramEntityBase> children = new List<ProgramEntityBase>(Classes);
            children.Sort((x, y) => { return -x.Susp.CompareTo(y.Susp); });
            return children;
        }

        public override string DisplayName
        {
            get
            {
                return ModuleName;
            }
        }


    }
}
