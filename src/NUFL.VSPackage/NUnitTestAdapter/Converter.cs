using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NUFL.Framework;
using NUFL.Service;
using NUFL.Framework.TestRunner;
using System.Diagnostics;

namespace Buaa.NUFL_VSPackage.NUnitTestAdapter
{
    public static class Converter
    {
        public static TestCase ConvertFromNUFLTestCase(NUFL.Framework.TestModel.TestCase test_case)
        {
            TestCase vs_test_case = new TestCase(test_case.FullyQualifiedName, TestExecutor.Uri, test_case.AssemblyPath)
            {
                DisplayName = test_case.DisplayName,
                CodeFilePath = test_case.CodeFilePath,
                LineNumber = test_case.LineNumber,
            };
            foreach (var property in test_case.Properties)
            {
                vs_test_case.Traits.Add(new Trait(property.Item1, property.Item2));
            }
            return vs_test_case;
        }
    }
}
