using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Buaa.NUFL_VSPackage.View
{
    /// <summary>
    /// Interaction logic for FLResultWrapper.xaml
    /// </summary>
    public partial class FLResultWrapper : UserControl
    {
        public FLResultWrapper()
        {
            InitializeComponent();
        }

        private void Navigate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(e.Parameter == null)
            {
                return;
            }
            Tuple<string, int> position = e.Parameter as Tuple<string, int>;
            if(position == null)
            {
                return;
            }
            string file = position.Item1;
            int line_number = position.Item2;
            Helpers.IDEHelper.NavigateTo(file, line_number);
        }

        private void OpenSetting_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SettingWindow window = new SettingWindow();
            window.ShowDialog();
        }

    }
}
