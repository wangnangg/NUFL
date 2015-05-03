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
    /// An method entity that can be instrumented
    /// </summary>
    public class Method:ProgramEntityBase
    {

        /// <summary>
        /// The MetadataToken used to identify this entity within the assembly
        /// </summary>
        public int MetadataToken { get; set; }

        /// <summary>
        /// The name of the method including namespace, return type and arguments
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// A list of sequence points that have been produced for this method
        /// </summary>
        public InstrumentationPoint[] Points { get; set; }

        public Method()
        {
            Points = new InstrumentationPoint[0];
        }



        /// <summary>
        /// Is this method a constructor
        /// </summary>
        [XmlAttribute("isConstructor")]
        public bool IsConstructor { get; set; }

        /// <summary>
        /// Is this method static
        /// </summary>
        [XmlAttribute("isStatic")]
        public bool IsStatic { get; set; }

        /// <summary>
        /// Is this method a property getter
        /// </summary>
        [XmlAttribute("isGetter")]
        public bool IsGetter { get; set; }
        
        /// <summary>
        /// Is this method a property setter
        /// </summary>
        [XmlAttribute("isSetter")]
        public bool IsSetter { get; set; }

        public bool Skipped { get; set; }


        public override IEnumerable<ProgramEntityBase> DirectChildren
        {
            get
            {
                if (Points == null)
                {
                    yield break;
                }
                foreach (var entry in Points)
                {
                    yield return entry;
                }

            }
        }

        protected override List<ProgramEntityBase> GetDirectChildrenSortedByCov()
        {
            List<ProgramEntityBase> children = new List<ProgramEntityBase>(Points);
            children.Sort((x, y) => { return x.CoveragePercent.CompareTo(y.CoveragePercent); });
            return children;
        }

        protected override List<ProgramEntityBase> GetDirectChildrenSortedBySusp()
        {
            List<ProgramEntityBase> children = new List<ProgramEntityBase>(Points);
            children.Sort((x, y) => { return -x.Susp.CompareTo(y.Susp); });
            return children;
        }

        public override string DisplayName
        {
            get
            {
                return Name;
            }
        }
    }
}
