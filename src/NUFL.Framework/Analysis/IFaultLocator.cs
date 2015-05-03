using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.TestModel;

namespace NUFL.Framework.Analysis
{

    public interface IFaultLocator
    {
        //func return true if the test case should be included.
        RankList GetRankList(Func<TestCase, bool> filter, string method);
    }
}
