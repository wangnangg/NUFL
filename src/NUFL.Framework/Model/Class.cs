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
using NUFL.Framework.CBFL;

namespace NUFL.Framework.Model
{
    /// <summary>
    /// An entity that contains methods
    /// </summary>
    public class Class:CBFLEntry
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

        public Position SourcePosition { get; set; }
        

        /// <summary>
        /// A list of methods that make up the class
        /// </summary>
        public Method[] Methods { get; set; }

        public bool Skipped { get; set; }

        public override IEnumerable<CBFLEntry> GetChildrenEnumerator()
        {
            foreach (var method in Methods)
            {
                foreach(var entry in method.GetChildrenEnumerator())
                {
                    yield return entry;
                }
            }
        }

    }
}
