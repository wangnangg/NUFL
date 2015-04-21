using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using NUFL.Framework.CBFL;

namespace NUFL.Framework.Model
{
    /// <summary>
    /// An instrumentable point
    /// </summary>
    public class InstrumentationPoint:CBFLEntry
    {
        public static readonly List<InstrumentationPoint> InstrumentPoints = new List<InstrumentationPoint>(8192);

        public static int Count
        {
            get { return InstrumentPoints.Count; }
        }

        public static void ResetAll()
        {
            foreach(var entry in InstrumentPoints)
            {
                entry.Reset();
            }
        }


        /// <summary>
        /// Initialise
        /// </summary>
        public InstrumentationPoint()
        {
            UniqueSequencePoint = (uint)InstrumentPoints.Count;
            InstrumentPoints.Add(this);
        }


        /// <summary>
        /// A unique number
        /// </summary>
        public UInt32 UniqueSequencePoint { get; set; }

        /// <summary>
        /// An order of the point within the method
        /// </summary>
        public UInt32 Ordinal { get; set; }

        /// <summary>
        /// The IL offset of the point
        /// </summary>
        public int Offset { get; set; }



       

        public Position SourcePosition { set; get; } 
    }
}
