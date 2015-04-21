using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using NUFL.Framework.ProfilerCommunication;
using System.Xml;
using NUFL.Framework.Model;
using NUFL.Framework.Persistance;
using NUFL.Framework.TestModel;

namespace NUFL.Framework.CBFL
{
    public class FaultLocator:IPersistance, IFaultLocator
    {
        List<TestCase> _tc_list = new List<TestCase>();
        List<Coverage> _cov_list = new List<Coverage>();
        FormulaFactory _formula_factory = new FormulaFactory();
        ModuleCache _module_cache = new ModuleCache();
        int _tc_count = 0;
        int _cov_count = 0;

        int ValidDataCount
        {
            get { return Math.Min(_tc_count, _cov_count); }
        }

        //保存
        public void PersistModule(Model.Module module)
        {
            //Debug.WriteLine("persisting " + module.FullName);
            _module_cache.AddModule(module);
        }

        public void PersistTestResult(string xml_result)
        {
            try
            {
                var tc = TestCase.ParseFromXml(xml_result);
                _tc_list.Add(tc);
                _tc_count += 1;
                //Debug.WriteLine(xml_result);
            }
            catch (Exception) { }
        }

        Coverage current;
        public void SaveCoverageData(UInt32[] data, UInt32 length)
        {
            for (int i = 0; i < length; i++)
            {
                UInt32 uid = data[i];
                //Debug.WriteLine(uid);
                if((uid & (UInt32)MSG_IdType.IT_MethodEnter) > 0)
                {
                    //this is method enter
                    current = new Coverage();
                    _cov_list.Add(current);
                    continue;
                }
                if((uid & (UInt32)MSG_IdType.IT_MethodLeave) > 0)
                {
                    //this is method leave
                    current = null;
                    _cov_count += 1;
                    continue;
                }

                if((uid & (UInt32)MSG_IdType.IT_Mask) > 0)
                {
                    //this is uid
                    current.Cover(uid);
                    continue;
                }
            }
        }

        int poll_time = 50;
        public void Commit(int result_count)
        {
            while(ValidDataCount < result_count)
            {
                System.Threading.Thread.Sleep(poll_time);
            }
            Match();
           
        }

        void Match()
        {
            for (int i = 0; i < ValidDataCount; i++)
            {
                _tc_list[i].Coverage = _cov_list[i];
            }
        }



        public List<RankEntry> GetRankList(Granularity gran, Func<TestCase, bool> filter, string method)
        {
            Match();
            var test_cases = _tc_list.GetRange(0,ValidDataCount).Where(filter);
            InputTestCases(test_cases);
            var formula = _formula_factory.CreateFormula(method);
            List<RankEntry> rank_list = null;
            switch(gran)
            {
                case Granularity.Statement:
                    rank_list = GetStatementRankList(formula);
                    break;
                case Granularity.Method:
                    rank_list = GetMethodRankList(formula);
                    break;
                case Granularity.Class:
                    rank_list = GetClassRankList(formula);
                    break;
                default:
                    return null;
            }
            rank_list.Sort((entry1, entry2) => { return -entry1.susp.CompareTo(entry2.susp); });
            return rank_list;
        }

        private List<RankEntry> GetStatementRankList(Func<CBFLEntry, float> formula)
        {
            List<RankEntry> rank_list = new List<RankEntry>();
            foreach(var entry in InstrumentationPoint.InstrumentPoints)
            {
                RankEntry rank_entry = new RankEntry();
                entry.Calculate(formula);
                rank_entry.Assign(entry);
                rank_list.Add(rank_entry);
            }
            return rank_list;
        }
        private List<RankEntry> GetClassRankList(Func<CBFLEntry, float> formula)
        {
            List<RankEntry> rank_list = new List<RankEntry>();
            foreach(var @class in _module_cache.GetClassEnumerator())
            {
                if (@class.Skipped)
                {
                    continue;
                }
                RankEntry rank_entry = new RankEntry();
                float max_susp = int.MinValue;
                CBFLEntry max_entry = null;
                foreach(var entry in @class.GetChildrenEnumerator())
                {
                    entry.Calculate(formula);
                    if(max_susp < entry.susp)
                    {
                        max_susp = entry.susp;
                        max_entry = entry;
                    }
                }
                if(max_entry == null)
                {
                    continue;
                }
                rank_entry.Assign(max_entry);
                rank_entry.Name = @class.FullName;
                rank_list.Add(rank_entry);
            }
            return rank_list;
        }

        private List<RankEntry> GetMethodRankList(Func<CBFLEntry, float> formula)
        {
            List<RankEntry> rank_list = new List<RankEntry>();
            foreach (var method in _module_cache.GetMethodEnumerator())
            {
                if(method.Skipped)
                {
                    continue;
                }
                RankEntry rank_entry = new RankEntry();
                float max_susp = int.MinValue;
                CBFLEntry max_entry = null;
                foreach (var entry in method.GetChildrenEnumerator())
                {
                    entry.Calculate(formula);
                    if (max_susp < entry.susp)
                    {
                        max_susp = entry.susp;
                        max_entry = entry;
                    }
                }
                if (max_entry == null)
                {
                    continue;
                }
                rank_entry.Assign(max_entry);
                rank_entry.Name = method.Name;
                rank_list.Add(rank_entry);
            }
            return rank_list;
        }
        private void InputTestCases(IEnumerable<TestCase> test_cases)
        {
            InstrumentationPoint.ResetAll();
            foreach(var tc in test_cases)
            {
                bool passed;
                if(tc.Result == "Passed")
                {
                    passed = true;
                }
                else if (tc.Result == "Failed")
                {
                    passed = false; 
                } else
                {
                    continue;
                }
                for(uint i=0; i<tc.Coverage.Count; i++)
                {
                    if (tc.Coverage.IsCovered(i))
                    {
                        InstrumentationPoint.InstrumentPoints[(int)i].Cover(true, passed);
                    }else
                    {
                        InstrumentationPoint.InstrumentPoints[(int)i].Cover(false, passed);
                    }
                }
            }
        }
    }
}
