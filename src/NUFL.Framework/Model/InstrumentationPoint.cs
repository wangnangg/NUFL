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
            base.Reset(false);
            a_ep = 0;
            a_ef = 0;
            a_np = 0;
            a_nf = 0;
            Susp = float.MinValue;
        }

        public InstrumentationPoint(Method method)
            : base(method)
        {
            Program program = (Program)Parent.Parent.Parent.Parent;
            program.RegisterPoint(this);
           
        }
        /// <summary>
        /// A unique number
        /// </summary>
        public UInt32 UniqueSequencePoint { get; set; }

        public int Offset { get; set; }

        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }

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

        protected override List<ProgramEntityBase> GetDirectChildrenSortedByCov()
        {
            return EmptyList;
        }
        protected override List<ProgramEntityBase> GetDirectChildrenSortedBySusp()
        {
            return EmptyList;
        }
       
    }
}
