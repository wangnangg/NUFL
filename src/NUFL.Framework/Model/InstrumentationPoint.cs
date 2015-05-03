using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using NUFL.Framework.Analysis;

namespace NUFL.Framework.Model
{
    /// <summary>
    /// An instrumentable point
    /// </summary>
    public class InstrumentationPoint:ProgramEntityBase
    {
        public int a_ep { private set; get; }
        public int a_ef { private set; get; }
        public int a_np { private set; get; }
        public int a_nf { private set; get; }
        public void Calculate(Func<InstrumentationPoint, float> formula)
        {
            Susp = formula(this);
        }
        public void Cover(bool covered, bool passed)
        {
            if (covered)
            {
                if (passed)
                {
                    a_ep += 1;
                }
                else
                {
                    a_ef += 1;
                }
            }
            else
            {
                if (passed)
                {
                    a_np += 1;
                }
                else
                {
                    a_nf += 1;
                }
            }
        }

        public override void Reset(bool recursive)
        {
            a_ep = 0;
            a_ef = 0;
            a_np = 0;
            a_nf = 0;
            base.Reset(false);
        }


        public static readonly List<InstrumentationPoint> InstrumentPoints = new List<InstrumentationPoint>(8192);

        public static int Count
        {
            get { return InstrumentPoints.Count; }
        }

        public static void ResetAllLeaf()
        {
            foreach(var entry in InstrumentPoints)
            {
                entry.Reset(false);
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

        public override int LeafChildrenCount
        {
            get
            {
                return 1;
            }
        }

        public override int CoveredLeafChildrenCount
        {
            get
            {
                return (a_ef + a_ep) > 0 ? 1 : 0;
            }
        }

        public override string DisplayName
        {
            get
            {
                return UniqueSequencePoint.ToString();
            }
        }
       
    }
}
