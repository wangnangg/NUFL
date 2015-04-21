using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.CBFL
{
    public class CBFLEntry
    {
        public int a_ep { private set; get; }
        public int a_ef { private set; get; }
        public int a_np { private set; get; }
        public int a_nf { private set; get; }
        public float susp { private set; get; }

        public void Calculate(Func<CBFLEntry, float> formula)
        {
            susp = formula(this);
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

        public void Assign(CBFLEntry other)
        {
            this.a_ep = other.a_ep;
            this.a_ef = other.a_ef;
            this.a_np = other.a_np;
            this.a_nf = other.a_nf;
            this.susp = other.susp;
        }

        public void Reset()
        {
            this.a_ep = 0;
            this.a_ef = 0;
            this.a_np = 0;
            this.a_nf = 0;
            this.susp = 0;
        }

        public virtual IEnumerable<CBFLEntry> GetChildrenEnumerator()
        {
            yield return this;
        }

    }
}
