using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
namespace NUFL.Framework.TestModel
{
    public interface ITestContainer
    {
        IEnumerable<TestCase> GetTestCaseEnumerator();
    }
    [XmlRoot("test-suite")]
    public class TestContainer:ITestContainer
    {
        [XmlIgnore]
        List<ITestContainer> _containers;
        [XmlAttribute("type")]
        public string Type { set; get; }
        [XmlAttribute("id")]
        public int Id { set; get; }
        [XmlAttribute("name")]
        public string Name { set; get; }
        [XmlAttribute("fullname")]
        public string FullName { set; get; }
        [XmlAttribute("runstate")]
        public string RunState { set; get; }
        [XmlAttribute("testcasecount")]
        public int TestCaseCount { set; get; }

        public TestContainer()
        {
            _containers = new List<ITestContainer>();
        }

        public IEnumerable<TestCase> GetTestCaseEnumerator()
        {
            foreach(var c in _containers)
            {
                 foreach(var test_case in c.GetTestCaseEnumerator())
                 {
                     yield return test_case;
                 }
            }
        }

        public int RunnableTestCaseCount
        {
            get
            {
                int count = 0;
                foreach (var tc in GetTestCaseEnumerator())
                {
                    if(tc.RunState == "Runnable")
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public static TestContainer ParseFromXml(string xml_string)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml_string);
            return ParseFromXml(doc.FirstChild);
        }
        public static TestContainer ParseFromXml(XmlNode node)
        {
            XmlSerializer serializer;
            serializer = new XmlSerializer(typeof(TestContainer));
            var container = (TestContainer)serializer.Deserialize(new XmlNodeReader(node));
            foreach(XmlNode childnode in node.ChildNodes)
            {
                if(childnode.Name == "test-suite")
                {
                    container.Add(TestContainer.ParseFromXml(childnode));
                } else if(childnode.Name == "test-case")
                {
                    container.Add(TestCase.ParseFromXml(childnode));
                }
            }
            return container;
        }

        public void Add(ITestContainer container)
        {
            _containers.Add(container);
        }

    }
}
