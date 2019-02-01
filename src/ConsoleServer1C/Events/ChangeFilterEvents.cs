using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer1C.Events
{
    public delegate void ChangeFilterFindBaseEvent();
    public delegate void ChangeFilterFindUserEvent();

    public class ChangeFilterEvents
    {
        public static event ChangeFilterFindBaseEvent ChangeFilterFindBaseEvent;
        public static void InvokeFindBaseEvent() => ChangeFilterFindBaseEvent?.Invoke();

        public static event ChangeFilterFindUserEvent ChangeFilterFindUserEvent;
        public static void InvokeFindUserEvent() => ChangeFilterFindUserEvent?.Invoke();
    }
}
