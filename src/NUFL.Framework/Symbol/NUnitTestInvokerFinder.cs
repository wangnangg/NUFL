using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace NUFL.Framework.Symbol
{
    public class NUnitTestInvokerFinder
    {
        public static List<int> GetInvokerTokens(string module_path, string module_name)
        {
            var tokens = new List<int>();
            if(module_name.Equals("NUnit.Framework", StringComparison.OrdinalIgnoreCase))
            {
                ModuleDefinition module = ModuleDefinition.ReadModule(module_path, 
                                        new ReaderParameters(ReadingMode.Deferred));
                foreach(var type in module.Types)
                {
                    if(type.FullName.Equals("NUnit.Framework.Internal.Execution.SimpleWorkItem", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach(var method in type.Methods)
                        {
                            if(method.Name.Equals("PerformWork", StringComparison.OrdinalIgnoreCase))
                            {
                                tokens.Add(method.MetadataToken.ToInt32());
                                return tokens;
                            }
                        }
                    }
                }    
            }

            return tokens;
        }
    }
}
