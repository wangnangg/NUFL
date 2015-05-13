using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NUFL.Framework.TestModel
{
    public static class TestConverters
    {
        public static List<TestCase> ConvertFromNUnitTestCase(string xml_string)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml_string);
            return ConvertFromNUnitTestCase(doc);
        }
        public static List<TestCase> ConvertFromNUnitTestCase(XmlNode node)
        {
            var result = new List<TestCase>();
            var ntc_nodes = node.SelectNodes("//test-case");
            foreach (XmlNode ntc in ntc_nodes)
            {
                TestCase tc = new TestCase()
                {
                    DisplayName = ntc.Attributes["name"].Value,
                    FullyQualifiedName = ntc.Attributes["fullname"].Value,
                    AssemblyPath = NUnitFindTestcaseAssembly(ntc),
                };
                result.Add(tc);
            }

            return result;
        }

        private static string NUnitFindTestcaseAssembly(XmlNode ntc)
        {
            XmlNode ass_node = ntc.SelectSingleNode("ancestor::test-suite[@type='Assembly']");
            return ass_node.Attributes["fullname"].Value;
        }

        public static TestResult ConvertFromNUnitTestResult(string xml_string)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml_string);
            return ConvertFromNUnitTestResult(doc);
        }

        public static TestResult ConvertFromNUnitTestResult(XmlNode node)
        {
            XmlNode test_case = node.SelectSingleNode("test-case");
            if(test_case == null)
            {
                throw new Exception("Invaild XML For Test Result.");
            }
            TestResult result = new TestResult()
            {
                FullyQualifiedName = test_case.Attributes["fullname"].Value,
                Duration = TimeSpan.FromSeconds(double.Parse(test_case.Attributes["duration"].Value)),
                Outcome = NUnitGetOutcome(test_case.Attributes["result"].Value),
            };
            if(result.Outcome == TestOutcome.Failed)
            {
                XmlNode stack_trace = test_case.SelectSingleNode("//stack-trace");
                XmlNode error_message = test_case.SelectSingleNode("//message");
                if(stack_trace != null)
                {
                    result.StackTrace = stack_trace.InnerText;
                }
                if(error_message != null)
                {
                    result.ErrorMessage = error_message.InnerText;
                }

            }
            return result;
        }

        private static TestOutcome NUnitGetOutcome(string outcome_str)
        {
            TestOutcome outcome = TestOutcome.None;
            switch(outcome_str)
            {
                case "Passed":
                    outcome = TestOutcome.Passed;
                    break;
                case "Failed":
                    outcome = TestOutcome.Failed;
                    break;
                case "Skipped":
                    outcome = TestOutcome.Skipped;
                    break;
                case "NotFound":
                    outcome = TestOutcome.NotFound;
                    break;
            }
            return outcome;
        }
    }
}
