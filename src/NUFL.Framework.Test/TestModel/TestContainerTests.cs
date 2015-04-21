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
    class TestContainerTests
    {
        string _container_xml = "<test-suite type=\"Assembly\" id=\"1003\" name=\"NUFL.TestTarget.dll\" fullname=\"E:\\workplace\\NUFL\\bin\\Debug\\MockAssembly\\NUFL.TestTarget.dll\" runstate=\"Runnable\" testcasecount=\"2\"><properties><property name=\"_PID\" value=\"2492\" /><property name=\"_APPDOMAIN\" value=\"test-domain-\" /></properties><test-suite type=\"TestSuite\" id=\"1004\" name=\"NUFL\" fullname=\"NUFL\" runstate=\"Runnable\" testcasecount=\"2\"><test-suite type=\"TestSuite\" id=\"1005\" name=\"TestTarget\" fullname=\"NUFL.TestTarget\" runstate=\"Runnable\" testcasecount=\"2\"><test-suite type=\"TestFixture\" id=\"1000\" name=\"Class1\" fullname=\"NUFL.TestTarget.Class1\" runstate=\"Runnable\" testcasecount=\"2\"><test-case id=\"1001\" name=\"Method1\" fullname=\"NUFL.TestTarget.Class1.Method1\" runstate=\"Runnable\" seed=\"1998102471\" /><test-case id=\"1002\" name=\"Method2\" fullname=\"NUFL.TestTarget.Class1.Method2\" runstate=\"Runnable\" seed=\"1801850340\" /></test-suite></test-suite></test-suite></test-suite>";


        [Test]
        public void TestContainerConstruction()
        {
            var container = TestContainer.ParseFromXml(_container_xml);
        }

        [Test]
        public void TestContainerEnumeration()
        {
            var container = TestContainer.ParseFromXml(_container_xml);
            foreach(var test_case in container.GetTestCaseEnumerator())
            {
                System.Diagnostics.Debug.WriteLine(test_case.FullName);
            }
        }
    }
}
