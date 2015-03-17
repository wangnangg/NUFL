//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using System.Collections.Generic;
using System.Linq;
using NUFL.Framework.Symbol;
using log4net;
using NUFL.Framework.Setting;

namespace NUFL.Framework.Model
{
    public class InstrumentationModelBuilderFactory : IInstrumentationModelBuilderFactory
    {
        private readonly IOption _commandLine;
        private readonly IFilter _filter;
        private readonly ILog _logger;

        public InstrumentationModelBuilderFactory(IOption commandLine, IFilter filter, ILog logger)
        {
            _commandLine = commandLine;
            _filter = filter;
            _logger = logger;
        }

        public IInstrumentationModelBuilder CreateModelBuilder(string modulePath, string moduleName)
        {
            var manager = new CecilSymbolManager(_commandLine, _filter, _logger);
            manager.Initialise(modulePath, moduleName);
            return new InstrumentationModelBuilder(manager);
        }

    }
}