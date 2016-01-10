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
using NUFL.Service;
using NUFL.Framework.Setting;

namespace NUFL.Framework.Analysis
{
    public class FaultLocator : IPersistance
    {
        List<TestResult> _tc_list = new List<TestResult>();
        List<Coverage> _cov_list = new List<Coverage>();
        FormulaFactory _formula_factory = new FormulaFactory();
        Program _program;
        int _tc_count = 0;
        int _cov_count = 0;

        public ISetting Option { set; get; }

        int ValidDataCount
        {
            get { return Math.Min(_tc_count, _cov_count); }
        }

        //保存
        public void PersistProgram(Program program)
        {
            if (_commited)
            {
                return;
            }
            //Debug.WriteLine("persisting " + module.FullName);
            _program = program;
        }
        bool _commited = false;
        public void PersistTestResult(NUFL.Framework.TestModel.TestResult result)
        {
            if (_commited)
            {
                return;
            }
            TestResult ourresult = new TestResult()
            {
                Passed = result.Outcome == TestModel.TestOutcome.Passed ? true : false,
            };
            _tc_list.Add(ourresult);
            _tc_count += 1;
        }

        Coverage current;
        public void SaveCoverageData(UInt32[] data, UInt32 length)
        {
            if(_commited)
            {
                return;
            }
            for (int i = 0; i < length; i++)
            {
                UInt32 uid = data[i];
                //Debug.WriteLine(uid);
                if((uid & (UInt32)MSG_IdType.IT_MethodEnter) > 0)
                {
                    //this is method enter
                    current = new Coverage(_program);
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
                    if(current != null)
                    {
                        current.Cover(uid);
                    }
                    continue;
                }
            }
        }

        public void Commit()
        {
            _commited = true;
            Match();
            var ranklist = GetRankList(Option.GetSetting<string>("fl_method"));
            _program.CalcSupsLevel(ranklist.GetSuspList(typeof(InstrumentationPoint)));
            if(GlobalEventManager.Instance != null)
            {
                GlobalEventManager.Instance.RaiseEvent(new GlobalEvent()
                {
                    Name = EventEnum.ProgramChanged,
                    Argument = _program,
                    Sender = this
                });
                GlobalEventManager.Instance.RaiseEvent(new GlobalEvent()
                {
                    Name = EventEnum.RankListChanged,
                    Argument = ranklist,
                    Sender = this
                });
            }

        }

        void Match()
        {
            for (int i = 0; i < ValidDataCount; i++)
            {
                _tc_list[i].Coverage = _cov_list[i];
            }
        }



        public RankList GetRankList(string method)
        {
            Match();
            var test_cases = _tc_list;
            var formula = _formula_factory.CreateFormula(method);
            InputTestCases(test_cases, formula);

            return new RankList(_program);
        }

        private void InputTestCases(IEnumerable<TestResult> test_cases, Func<InstrumentationPoint, float> formula)
        {
            _program.Reset(true);
            foreach(var tc in test_cases)
            {
                bool passed = tc.Passed;
                for(uint i=0; i<tc.Coverage.Count; i++)
                {
                    if (tc.Coverage.IsCovered(i))
                    {
                         _program.Points[(int)i].Cover(true, passed);
                    }else
                    {
                        _program.Points[(int)i].Cover(false, passed);
                    }
                }
            }
            foreach (var entry in _program.Points)
            {
                entry.Calculate(formula);
            }
        }

    }
}
