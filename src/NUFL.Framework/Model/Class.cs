//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using NUFL.Framework.Analysis;

namespace NUFL.Framework.Model
{
    /// <summary>
    /// An entity that contains methods
    /// </summary>
    public class Class:ProgramEntityBase
    {
        /// <summary>
        /// instantiate
        /// </summary>
        public Class()
        {
            Methods = new Method[0];
        }

        /// <summary>
        /// The full name of the class
        /// </summary>
        public string FullName { get; set; }

        

        /// <summary>
        /// A list of methods that make up the class
        /// </summary>
        public Method[] Methods { get; set; }

        public bool Skipped { get; set; }


        public override IEnumerable<ProgramEntityBase> DirectChildren
        {
            get
            {
                foreach(var method in Methods)
                {
                    if(method.Skipped || method.Points == null || method.Points.Length == 0)
                    {
                        continue;
                    }
                    yield return method;
                }
                yield break;
            }
        }

        protected override List<ProgramEntityBase> GetDirectChildrenSortedByCov()
        {
            List<ProgramEntityBase> children = new List<ProgramEntityBase>(Methods);
            children.Sort((x, y) => { return x.CoveragePercent.CompareTo(y.CoveragePercent); });
            return children;
        }

        protected override List<ProgramEntityBase> GetDirectChildrenSortedBySusp()
        {
            List<ProgramEntityBase> children = new List<ProgramEntityBase>(Methods);
            children.Sort((x, y) => { return -x.Susp.CompareTo(y.Susp); });
            return children;
        }


        public override string DisplayName
        {
            get
            {
                return FullName;
            }
        }

    }
}
