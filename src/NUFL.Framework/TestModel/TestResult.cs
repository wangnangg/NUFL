using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.TestModel
{
    [Serializable]
    public enum TestOutcome
    {
        Failed,
        Passed,
        Skipped,
        None,
        NotFound,
    }
    [Serializable]
    public class TestResult
    {
        public string FullyQualifiedName { set; get; }
        public TimeSpan Duration { set; get; }
        public string ErrorMessage { set; get; }
        public string StackTrace { set; get; }
        public TestOutcome Outcome { set; get; }
    }
}
