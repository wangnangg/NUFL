using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.Model
{
    public class ModuleCache
    {
        List<Module> _modules;
        public ModuleCache()
        {
            _modules = new List<Module>();
        }
        public void AddModule(Module module)
        {
            //make sure no redundancy
            foreach(var m in _modules)
            {
                if(m.ModuleHash == module.ModuleHash)
                {
                    m.Aliases.Add(module.FullName);
                    return;
                }
            }
            _modules.Add(module);
        }

        public Module RetrieveModule(string module_path)
        {
            foreach(var module in _modules)
            {
                if(module.Aliases.Contains(module_path))
                {
                    return module;
                }
            }

            return null;
        }

        public Method RetrieveMethod(string module_path, int function_token)
        {
            Module module = RetrieveModule(module_path);
            foreach(var @class in module.Classes)
            {
                foreach(var method in @class.Methods)
                {
                    if(method.MetadataToken == function_token)
                    {
                        return method;
                    }
                }
            }

            return null;
        }

      



    }
}
