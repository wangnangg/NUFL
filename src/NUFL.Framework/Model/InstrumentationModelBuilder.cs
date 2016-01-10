//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUFL.Framework.Symbol;
using NUFL.Framework.Setting;
using System.Collections.Generic;

namespace NUFL.Framework.Model
{
    public class InstrumentationModelBuilder : IInstrumentationModelBuilder
    {
        private readonly CecilSymbolManager _symbolManager;

        /// <summary>
        /// Standard constructor
        /// </summary>
        /// <param name="symbolManager">the symbol manager that will provide the data</param>
        public InstrumentationModelBuilder(string module_path, string module_name, IOption option, IFilter filter, IEnumerable<string> pdb_directories, Program program)
        {
            _symbolManager = new CecilSymbolManager(option, filter, pdb_directories, program);
            _symbolManager.Initialise(module_path, module_name);
        }

        public Module BuildModuleModel()
        {
            var module = CreateModule();
            return module;
        }

        private Module CreateModule()
        {
            var hash = string.Empty;
            if (System.IO.File.Exists(_symbolManager.ModulePath))
            {
                hash = HashFile(_symbolManager.ModulePath);
            }
            var module = new Module
                             {
                                 ModuleName = _symbolManager.ModuleName,
                                 FullName = _symbolManager.ModulePath,
                                 ModuleHash = hash
                             };
            module.Aliases.Add(_symbolManager.ModulePath);
            
            module.Classes = _symbolManager.GetInstrumentableTypes();
            foreach (var @class in module.Classes)
            {
                BuildClassModel(@class);
            }
            return module;
        }

        private string HashFile(string sPath)
        {
            using (var sr = new StreamReader(sPath))
            using (var prov = new SHA1CryptoServiceProvider())
            {
                return BitConverter.ToString(prov.ComputeHash(sr.BaseStream));
            }
        }

        public bool CanInstrument
        {
            get { return _symbolManager.SourceAssembly != null; }
        }

        private void BuildClassModel(Class @class)
        {
            if (@class.Skipped) 
                return;
            var methods = _symbolManager.GetMethodsForType(@class);

            foreach (var method in methods)
            {
                if (!method.Skipped)
                {
                    method.Points = _symbolManager.GetSequencePointsForToken(method.MetadataToken);
                }
                //method.CyclomaticComplexity = _symbolManager.GetCyclomaticComplexityForToken(method.MetadataToken);
            }

            @class.Methods = methods;
        }
    }
}
