using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NUFL.Framework.Setting;
using NUFL.GUI.ViewModel;
using NUFL.GUI.View;
namespace NUFL.GUI.Command
{
    public class OpenSettingCommand:ICommand
    {

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var view_model = new SettingViewModel((ISetting)parameter);
            var window =  new SettingWindow();
            window.DataContext = view_model;
            window.ShowDialog();
               
        }
    }
}
