using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUFL.Framework.Model
{
    public class Program : ProgramEntityBase
    {
        List<Module> _modules;
        public Program()
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

        public IEnumerable<Module> GetModuleEnumerator()
        {
            return _modules;
        }

        public IEnumerable<Class> GetClassEnumerator()
        {
            foreach(var module in _modules)
            {
                foreach(var @class in module.DirectChildren)
                {
                    yield return @class as Class;
                }
            }
            yield break;
        }

        public IEnumerable<Method> GetMethodEnumerator()
        {
            foreach(var module in _modules)
            {
                foreach (var @class in module.DirectChildren)
                {
                    foreach(var method in @class.DirectChildren)
                    {
                        yield return method as Method;
                    }
                }
            }
            yield break;
        }

        public IEnumerable<InstrumentationPoint> GetPointEnumerator()
        {
            foreach(var point in InstrumentationPoint.InstrumentPoints)
            {
                yield return point;
            }
        }


        public override IEnumerable<ProgramEntityBase> DirectChildren
        {
            get
            {
                foreach(var module in _modules)
                {
                    if(module.Skipped)
                    {
                        continue;
                    }
                    yield return module;
                }
                yield break;
            }
        }

        protected override List<ProgramEntityBase> GetDirectChildrenSortedByCov()
        {
            List<ProgramEntityBase> children = new List<ProgramEntityBase>(_modules);
            children.Sort((x, y) => { return x.CoveragePercent.CompareTo(y.CoveragePercent); });
            return children;
        }
        protected override List<ProgramEntityBase> GetDirectChildrenSortedBySusp()
        {
            List<ProgramEntityBase> children = new List<ProgramEntityBase>(_modules);
            children.Sort((x, y) => { return -x.Susp.CompareTo(y.Susp); });
            return children;
        }
        public override string DisplayName
        {
            get
            {
                return "Program";
            }
        }






    }
}
