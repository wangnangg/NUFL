﻿using System;
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
                    return (float)entry.a_ef - (float)entry.a_ep / (float)100;
                };
        }
    }
}
