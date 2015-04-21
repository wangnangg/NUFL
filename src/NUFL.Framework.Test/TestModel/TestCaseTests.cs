using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUFL.Framework.TestModel;

namespace NUFL.Framework.Test.TestModel
{
    [TestFixture]
    class TestCaseTests
    {
        string _xml_test_case_result = "<test-case id=\"1002\" name=\"Method2\" fullname=\"NUFL.TestTarget.Class1.Method2\" runstate=\"Runnable\" seed=\"1801850340\" result=\"Passed\" start-time=\"2015-04-21 03:07:00Z\" end-time=\"2015-04-21 03:07:00Z\" duration=\"0.042000\" asserts=\"1\" />";
        string _xml_test_case_discover = "<test-case id=\"1002\" name=\"Method2\" fullname=\"NUFL.TestTarget.Class1.Method2\" runstate=\"Runnable\" seed=\"1801850340\" />";

        [Test]
        public void TestCaseResultConstruction()
        {
            TestCase tc = TestCase.ParseFromXml(_xml_test_case_result);

        }
        [Test]
        public void TestCaseDiscoverConstruction()
        {
            TestCase tc = TestCase.ParseFromXml(_xml_test_case_discover);

        }
    }
}
