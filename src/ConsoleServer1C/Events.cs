using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer1C
{
    internal delegate void UpdateListBasesMainWindowEvent();
    internal class UpdateInfoMainWindowEvents : EventArgs
    {
        internal static List<Models.InfoBase> InfoBases { get; set; }

        internal static event UpdateListBasesMainWindowEvent UpdateListBasesMainWindowEvent;

        internal static void EvokeUpdateListBasesMainWindow()
            => UpdateListBasesMainWindowEvent?.Invoke();
    }
}
