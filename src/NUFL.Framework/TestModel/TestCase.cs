using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using NUFL.Framework.Analysis;

namespace NUFL.Framework.TestModel
{
    [XmlRoot("test-case")]
    public class TestCase:ITestContainer
    {
        [XmlAttribute("id")]
        public int Id { set; get; }
        [XmlAttribute("name")]
        public string Name { set; get; }
        [XmlAttribute("fullname")]
        public string FullName { set; get; }
        [XmlAttribute("runstate")]
        public string RunState { set; get; }
        [XmlAttribute("seed")]
        public long Seed { set; get; }
        [XmlAttribute("result")]
        public string Result { set; get; }
        [XmlAttribute("start-time")]
        public string StartTime { set; get; }
        [XmlAttribute("end-time")]
        public string EndTime { set; get; }
        [XmlAttribute("duration")]
        public double Duration { set; get; }
        [XmlAttribute("asserts")]
        public int Asserts { set; get; }

        [XmlIgnore]
        public Coverage Coverage { set; get; }
        public static TestCase ParseFromXml(XmlNode node)
        {
             XmlSerializer serializer;
            serializer = new XmlSerializer(typeof(TestCase));
            var testcase = (TestCase)serializer.Deserialize(new XmlNodeReader(node));
            return testcase;

        }
        public static TestCase ParseFromXml(string xml_string)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml_string);
            return ParseFromXml(doc.FirstChild);

        }

        public IEnumerable<TestCase> GetTestCaseEnumerator()
        {
            yield return this;
        }
    }
}
