using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUFL.GUI.ViewModel;
using NUFL.Service;
using NUFL.Framework.Analysis;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using NUFL.Framework.Model;
namespace NUFL.GUI.Model
{
    public interface IFLResultPresenter
    {
        void Present(RankList rank_list);
    }
    public class FLResultPresentService :  GlobalServiceBase,IFLResultPresenter
    {
        public FLResultViewModel ViewModel { set; get; }
        Dispatcher _dispatcher;
        
        public FLResultPresentService(Dispatcher dispatcher)
        {
            ViewModel = new FLResultViewModel();
            _dispatcher = dispatcher;
        }


        public void Present(RankList rank_list)
        {
            ViewModel.DataSource = rank_list;
        }
    }

}
