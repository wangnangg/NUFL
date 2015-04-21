using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Model;

namespace NUFL.Framework.CBFL
{
    public class FormulaFactory
    {
        public Func<CBFLEntry, float> CreateFormula(string method)
        {
            return (entry) =>
                {
                    return entry.a_ef - entry.a_ep / 100;
                };
        }
    }
}
