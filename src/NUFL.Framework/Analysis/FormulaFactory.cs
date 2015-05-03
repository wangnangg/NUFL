using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Model;

namespace NUFL.Framework.Analysis
{
    public class FormulaFactory
    {
        public Func<InstrumentationPoint, float> CreateFormula(string method)
        {
            return (entry) =>
                {
                    return entry.a_ef - entry.a_ep / 100;
                };
        }
    }
}
