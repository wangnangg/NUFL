using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.IO;
namespace NUFL.Framework.Persistance
{
    [XmlRoot("test-case")]
    public class TestCase
    {
        [XmlAttribute("id")]
        public int id;
        [XmlAttribute("name")]
        public string name;
        [XmlAttribute("fullname")]
        public string fullname;
        [XmlAttribute("result")]
        public string result;
        [XmlAttribute("duration")]
        public float duration;

        public string message;
        public string stackTrace;



        public static TestCase ParseFromXmlString(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TestCase));
            TestCase tc = null;
            using(TextReader reader = new StringReader(xml))
            {
                try
                {
                    tc = (TestCase)serializer.Deserialize(reader);
                }
                catch (Exception) { }
            }

            return tc;

        }


    }
}
