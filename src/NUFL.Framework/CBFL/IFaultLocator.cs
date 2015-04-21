using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.TestModel;

namespace NUFL.Framework.CBFL
{
    public enum Granularity
    {
        Statement,
        Method,
        Class
    }

    public interface IFaultLocator
    {
        //func return true if the test case should be included.
        List<RankEntry> GetRankList(Granularity gran, Func<TestCase, bool> filter, string method);
    }
}
