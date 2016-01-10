using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NUFL.GUI.Command
{
    public static class FLResultViewCommands
    {
        public static RoutedUICommand Navigate { set; get; }
        public static RoutedUICommand OpenSetting { set; get; }

        static FLResultViewCommands()
        {
            Navigate = new RoutedUICommand
                (
                        "Navigate to source code.",
                        "Navigate",
                        typeof(FLResultViewCommands)
                );
            OpenSetting = new RoutedUICommand
                (
                        "Open setting page",
                        "OpenSetting",
                        typeof(FLResultViewCommands)
                );
        }

        //Define more commands here, just like the one above
    }
}
