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

    public class FLResultPresenter : GlobalInstanceServiceBase, IFLResultPresenter
    {
        public FLResultViewModel ViewModel { set; get; }
        public Dispatcher UIDispatcher { set; get; }
        
        public FLResultPresenter()
        {
            ViewModel = new FLResultViewModel();
        }


        public void Present(RankList rank_list)
        {
            if (ViewModel != null)
            {
                ViewModel.DataSource = rank_list;
            }
          
        }
    }

}
