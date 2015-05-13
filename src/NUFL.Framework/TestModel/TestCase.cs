using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using NUFL.Framework.Analysis;
using NUFL.Framework.Model;

namespace NUFL.Framework.TestModel
{
    [Serializable]
    public class TestCase
    {
        public string CodeFilePath { get; set; }
        public string DisplayName { get; set; }
        public string FullyQualifiedName { get; set; }
        public int LineNumber { get; set; }
        public string AssemblyPath { get; set; }
    }

    //[Serializable]
    //[XmlRoot("test-case")]
    //public class NUnitTestCase:ITestContainer
    //{
    //    [XmlAttribute("id")]
    //    public int Id { set; get; }
    //    [XmlAttribute("name")]
    //    public string Name { set; get; }
    //    [XmlAttribute("fullname")]
    //    public string FullName { set; get; }
    //    [XmlAttribute("runstate")]
    //    public string RunState { set; get; }
    //    [XmlAttribute("seed")]
    //    public long Seed { set; get; }
    //    [XmlAttribute("result")]
    //    public string Result { set; get; }
    //    [XmlAttribute("start-time")]
    //    public string StartTime { set; get; }
    //    [XmlAttribute("end-time")]
    //    public string EndTime { set; get; }
    //    [XmlAttribute("duration")]
    //    public double Duration { set; get; }
    //    [XmlAttribute("asserts")]
    //    public int Asserts { set; get; }

    //    [XmlElement("failure")]
    //    public Failure Failure { set; get; }

    //    [XmlIgnore]
    //    public string SourceAssembly 
    //    {
    //        get
    //        {
    //            var parent = this.Parent;
    //            while(parent != null)
    //            {
    //                var container = (TestContainer)parent;
    //                if(container.Type == "Assembly")
    //                {
    //                    return container.FullName;
    //                }
    //                parent = parent.Parent;
    //            }
    //            return "noname";
    //        }
    //    }

    //    [XmlIgnore]
    //    public Coverage Coverage { set; get; }
    //    public static NUnitTestCase ParseFromXml(XmlNode node)
    //    {
    //         XmlSerializer serializer;
    //        serializer = new XmlSerializer(typeof(NUnitTestCase));
    //        var testcase = (NUnitTestCase)serializer.Deserialize(new XmlNodeReader(node));
    //        return testcase;

    //    }
    //    public static NUnitTestCase ParseFromXml(string xml_string)
    //    {
    //        XmlDocument doc = new XmlDocument();
    //        doc.LoadXml(xml_string);
    //        return ParseFromXml(doc.FirstChild);

    //    }

    //    public IEnumerable<NUnitTestCase> GetTestCaseEnumerator()
    //    {
    //        yield return this;
    //    }

    //    [XmlIgnore]
    //    public ITestContainer Parent
    //    {
    //        get;
    //        set;
    //    }
    //}
}
