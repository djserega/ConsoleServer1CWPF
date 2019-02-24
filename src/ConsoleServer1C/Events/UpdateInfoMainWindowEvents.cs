using System;
using System.Collections.Generic;

namespace ConsoleServer1C.Events
{
    internal delegate void UpdateListBasesMainWindowEvent(bool updateSessions);
    internal class UpdateInfoMainWindowEvents : EventArgs
    {
        internal static List<Models.InfoBase> InfoBases { get; set; }

        internal static event UpdateListBasesMainWindowEvent UpdateListBasesMainWindowEvent;

        internal static void EvokeUpdateListBasesMainWindow(bool updateSessions)
            => UpdateListBasesMainWindowEvent?.Invoke(updateSessions);
    }
}
