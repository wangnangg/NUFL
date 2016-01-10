using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Setting;

namespace NUFL.Framework.Model
{
    public class Program : ProgramEntityBase
    {
        List<Module> _modules = new List<Module>();
        Dictionary<string, Module> _path_module_mapping = new Dictionary<string,Module>();
        public List<InstrumentationPoint> Points { get; private set; }
        public SourceFileCollection Files { get; private set; }
        IEnumerable<string> _pdb_directories;
        ProgramEntityFilter _filter;

        public void RegisterPoint(InstrumentationPoint point)
        {
            point.UniqueSequencePoint = (uint)Points.Count;
            Points.Add(point);
        }

        public Program(ProgramEntityFilter filter, IEnumerable<string> pdb_directories):base(null)
        {
            _filter = filter;
            _pdb_directories = pdb_directories;
            Points = new List<InstrumentationPoint>();
            Files = new SourceFileCollection();
        }


        public Module AddModule(string module_path, string fullname)
        {
            if(_path_module_mapping.ContainsKey(module_path))
            {
                return _path_module_mapping[module_path];
            }
            var module = new Module(module_path, fullname, _filter, _pdb_directories, this);
            _modules.Add(module);
            _path_module_mapping.Add(module_path, module);
            return module;
        }

        public void FindMethodSourcePosition(string module_path, string class_name, string method_name,
            out SourceFile file, out int? line_number)
        {
            file = null;
            line_number = null;
            var module = _path_module_mapping[module_path];
            module.BuildModule(false);
            foreach(var @class in module.Classes)
            {
                if(@class.FullName == class_name)
                {
                    foreach(var method in @class.Methods)
                    {
                        if(method.Name == method_name)
                        {
                            file = method.File;
                            line_number = method.StartLine;
                        }
                    }
                }
            }
        }
        
        public List<InstrumentationPoint> GetSequencePointsForMethod(string module_path, int token)
        {
            if(!_path_module_mapping.ContainsKey(module_path))
            {
                return new List<InstrumentationPoint>();
            }
            var points = _path_module_mapping[module_path].GetSequencePointsForMethod(token);
            if(points == null)
            {
                return new List<InstrumentationPoint>();
            }
            return points;
        }

        public IEnumerable<Module> GetModuleEnumerator()
        {
            foreach (var module in DirectChildren)
            {
                yield return module as Module;
            }
        }

        public IEnumerable<Class> GetClassEnumerator()
        {
            foreach (var module in GetModuleEnumerator())
            {
                foreach (var @class in module.DirectChildren)
                {
                    yield return @class as Class;
                }
                
            }
            yield break;
        }

        public IEnumerable<Method> GetMethodEnumerator()
        {
            foreach (var @class in GetClassEnumerator())
            {
                foreach (var method in @class.DirectChildren)
                {
                    yield return method as Method;
                }
            }
            yield break;
        }

        public IEnumerable<InstrumentationPoint> GetPointEnumerator()
        {
            foreach (var point in Points)
            {
                yield return point;
            }
        }


        public void CalcSupsLevel(IEnumerable<ProgramEntityBase> sorted_points)
        {
            int level = 0;
            float current_susp = float.MaxValue;
            foreach (var point in sorted_points)
            {
                if(point.Susp < current_susp)
                {
                    current_susp = point.Susp;
                    level -= 1;
                }
                point.SuspLevel = level;
            }
            int offset = -level + 1;
            if (offset > 6)
            {
                offset = 6;
            }
            foreach(var point in Points)
            {
                point.SuspLevel += offset;
            }
        }


        public override IEnumerable<ProgramEntityBase> DirectChildren
        {
            get
            {
                foreach (var module in _modules)
                {
                    if (module.Skipped)
                    {
                        continue;
                    }
                    yield return module;
                }
                yield break;
            }
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
