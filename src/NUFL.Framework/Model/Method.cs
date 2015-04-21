//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System.Collections.Generic;
using System.Xml.Serialization;
using NUFL.Framework.CBFL;

namespace NUFL.Framework.Model
{
    /// <summary>
    /// An method entity that can be instrumented
    /// </summary>
    public class Method:CBFLEntry
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
        public Position SourcePosition { get; set; }

        public override IEnumerable<CBFLEntry> GetChildrenEnumerator()
        {
            if(Points == null)
            {
                yield break;
            }
            foreach(var entry in Points)
            {
                yield return entry;
            }
        }
    }
}
