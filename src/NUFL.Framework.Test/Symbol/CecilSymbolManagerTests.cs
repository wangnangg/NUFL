using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUFL.Framework.Symbol;
using NUFL.Framework.Model;
using NUFL.Framework.Setting;
using System.Security.Cryptography;
using log4net;

namespace NUFL.Framework.Test.Symbol
{
    [TestFixture]
    class CecilSymbolManagerTests
    {
        CommandLineParser _commandline;
        IFilter _filter;
        ILog _logger;
        CecilSymbolManager _symbol_manger;
        [SetUp]
        public void Setup()
        {
            _commandline = new CommandLineParser(new string[]{
                 "-target:NUFL.TestTarget.dll",
                 "-targetargs:",
                 "-register:user",
                 "-filter:+[*]*"
             });
            _commandline.ExtractAndValidateArguments();
            _filter = Filter.BuildFilter(_commandline);
            _logger = LogManager.GetLogger("test");
            _symbol_manger = new CecilSymbolManager(_commandline, _filter, _logger);
            _symbol_manger.Initialise(".\\NUFL.TestTarget.dll","NUFL.TestTarget");
        }
        [Test]
        public void SymbolSimpleReading()
        {
            var files = _symbol_manger.GetFiles();
            var entities = _symbol_manger.GetInstrumentableTypes();
            Assert.IsNotEmpty(files);
            Assert.IsNotEmpty(entities);
        }

        [Test]
        public void SymbolCompletedReading()
        {
            string hash = HashFile(_symbol_manger.ModulePath);
            Module module = new Module 
                {
                    ModuleName = _symbol_manger.ModuleName,
                    FullName = _symbol_manger.ModulePath,
                    ModuleHash = hash
                };
            module.Aliases.Add(_symbol_manger.ModulePath);

            module.Files = _symbol_manger.GetFiles();
            module.Classes = _symbol_manger.GetInstrumentableTypes();
            foreach(var @class in module.Classes)
            {
                BuildClassModel(@class, module.Files);
            }
            Assert.IsNotEmpty(module.Files);
            Assert.IsNotEmpty(module.Classes);
            Assert.IsNotEmpty(module.Classes[1].Methods);
            Assert.IsNotEmpty(module.Classes[1].Methods[0].SequencePoints);

        }

        private void BuildClassModel(Class @class, File[] files)
        {
            if (@class.ShouldSerializeSkippedDueTo())
                return;
            var methods = _symbol_manger.GetMethodsForType(@class, files);

            foreach (var method in methods)
            {
                if (!method.ShouldSerializeSkippedDueTo())
                {
                    method.SequencePoints = _symbol_manger.GetSequencePointsForToken(method.MetadataToken);
                    if (method.SequencePoints != null && method.SequencePoints.Length != 0)
                    {
                        method.MethodPoint = method.SequencePoints.FirstOrDefault(pt => pt.Offset == 0);
                        method.BranchPoints = _symbol_manger.GetBranchPointsForToken(method.MetadataToken);
                    }
                    method.MethodPoint = method.MethodPoint ?? new InstrumentationPoint();
                    method.BranchPoints = method.BranchPoints ?? new BranchPoint[0];
                }
                method.CyclomaticComplexity = _symbol_manger.GetCyclomaticComplexityForToken(method.MetadataToken);
            }

            @class.Methods = methods;
        }


        private string HashFile(string sPath)
        {
            using (var sr = new System.IO.StreamReader(sPath))
            using (var prov = new SHA1CryptoServiceProvider())
            {
                return BitConverter.ToString(prov.ComputeHash(sr.BaseStream));
            }
        }
    }
}
