using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Model;
namespace NUFL.Framework.Analysis
{
    public class RankList:MarshalByRefObject
    {
        Program _program;
        Comparison<ProgramEntityBase> _susp_comparison = (x, y) => { return -x.Susp.CompareTo(y.Susp); };
        Comparison<ProgramEntityBase> _cov_comparison = (x, y) => { return x.CoveragePercent.CompareTo(y.CoveragePercent); };
        Dictionary<Type, List<ProgramEntityBase>> _susp_result_map = new Dictionary<Type, List<ProgramEntityBase>>();
        Dictionary<Type, List<ProgramEntityBase>> _cov_result_map = new Dictionary<Type, List<ProgramEntityBase>>();
        Dictionary<Type, IEnumerable<ProgramEntityBase>> _source_map = new Dictionary<Type, IEnumerable<ProgramEntityBase>>();

        public RankList(Program program)
        {
            _program = program;
            _source_map.Add(typeof(Program), new Program[] { _program });
            _source_map.Add(typeof(Module), _program.GetModuleEnumerator());
            _source_map.Add(typeof(Class), _program.GetClassEnumerator());
            _source_map.Add(typeof(Method), _program.GetMethodEnumerator());
            _source_map.Add(typeof(InstrumentationPoint), _program.GetPointEnumerator());
        }

        public List<ProgramEntityBase> GetSuspList(Type type)
        {
            if(_susp_result_map.ContainsKey(type))
            {
                return _susp_result_map[type];
            }
            if (!_source_map.ContainsKey(type))
            {
                return null;
            }
            List<ProgramEntityBase> result = new List<ProgramEntityBase>(_source_map[type]);
            result.Sort(_susp_comparison);
            _susp_result_map[type] = result;
            return result;
        }
        public List<ProgramEntityBase> GetCovList(Type type)
        {
            if(_cov_result_map.ContainsKey(type))
            {
                return _cov_result_map[type];
            }
            if(!_source_map.ContainsKey(type))
            {
                return null;
            }
            List<ProgramEntityBase> result = new List<ProgramEntityBase>(_source_map[type]);
            result.Sort(_cov_comparison);
            _cov_result_map[type] = result;
            return result;
        }


    }
}
