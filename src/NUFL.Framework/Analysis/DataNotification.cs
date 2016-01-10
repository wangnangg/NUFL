using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.Framework.Model;
namespace NUFL.Framework.Analysis
{
    public interface IRankListEvent
    {
        event Action<RankList> RankListChanged;

        void RaiseRankListChangedEvent(RankList ranklist);
    }
    public interface IMetaDataEvent
    {
        event Action<Program> MetaDataChanged;

        void RaiseMetaDataChangedEvent(Program program);
    }


    public class DataNotification : IRankListEvent, IMetaDataEvent
    {

        Action<RankList> _rank_list_changed;
        public event Action<RankList> RankListChanged
        {
            add
            {
                _rank_list_changed += value;
                if(_last_rank_list != null)
                {
                    value(_last_rank_list);
                }
            }

            remove
            {
                _rank_list_changed -= value;
            }
        }
        RankList _last_rank_list = null;
        public void RaiseRankListChangedEvent(RankList ranklist)
        {
            _last_rank_list = ranklist;
            if (_rank_list_changed != null)
            {
                _rank_list_changed(ranklist);
            }
        }

        Program _last_program = null;
        Action<Program> _meta_data_changed;
        public event Action<Program> MetaDataChanged
        {
            add
            {
                _meta_data_changed += value;
                if (_last_program != null)
                {
                    value(_last_program);
                }
            }

            remove
            {
                _meta_data_changed -= value;
            }
        }

        public void RaiseMetaDataChangedEvent(Program program)
        {
            _last_program = program;
            if (_meta_data_changed != null)
            {
                _meta_data_changed(program);
            }
        }
    }
}
