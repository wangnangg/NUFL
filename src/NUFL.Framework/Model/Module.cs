using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Setting;
using NUFL.Framework.Symbol;
using System.Diagnostics;
namespace NUFL.Framework.Model
{
    public class Module : ProgramEntityBase
    {

        Dictionary<int, Method> _token_method_mapping;
        ProgramEntityFilter _filter;
        SymbolReader _symbol_reader;

       
        public string ModulePath { set; get; }
        public string FullName { set; get; }
        public List<Class> Classes { set; get; }

        public Module(string module_path, string fullname, ProgramEntityFilter filter, IEnumerable<string> pdb_directories, Program program):base(program)
        {
            ModulePath = module_path;
            _filter = filter;
            FullName = fullname;
            Skipped = !_filter.UseAssembly(this);
            if (!Skipped)
            {
                _symbol_reader = new SymbolReader(module_path, filter, pdb_directories, this);
                if(_symbol_reader.SourceAssembly == null)
                {
                    Skipped = true;
                    _symbol_reader = null;
                }
            }
            
        }
        public List<InstrumentationPoint> GetSequencePointsForMethod(int token)
        {
            if(Skipped)
            {
                return null;
            }
            BuildModule(true);
            if(_token_method_mapping.ContainsKey(token))
            {
                var method = _token_method_mapping[token];
                return method.Points;
            }
            return null;
        }



        bool _half_built = false;
        bool _full_built = false;
        public void BuildModule(bool full)
        {
            if(_full_built)
            {
                return;
            }
            if (!_half_built)
            {
                Classes = _symbol_reader.GetClasses();
                _half_built = true;
            }
            if (full)
            {
                _token_method_mapping = new Dictionary<int, Method>();
                foreach (var @class in Classes)
                {
                    if (@class.Skipped)
                    {
                        continue;
                    }
                    foreach (var method in @class.Methods)
                    {
                        if (method.Skipped)
                        {
                            continue;
                        }
                        _token_method_mapping.Add(method.MetadataToken, method);
                        method.BuildMethod();
                    }
                }
                _full_built = true;
            }
        }



        public override IEnumerable<ProgramEntityBase> DirectChildren
        {
            get
            {
                if (!_full_built)
                {
                    yield break;
                }
                foreach (var @class in Classes)
                {
                    if (@class.Skipped)
                    {
                        continue;
                    }
                    yield return @class;
                }
                yield break;
            }
        }


        public override string DisplayName
        {
            get
            {
                return FullName;
            }
        }
    }
}
