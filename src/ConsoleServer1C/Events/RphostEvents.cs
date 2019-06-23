using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer1C.Events
{
    public delegate void NewObject(Models.RphostObject obj);
    public class RphostEvents
    {
        public static event NewObject NewObject;

        public static void Created(Models.RphostObject obj) => NewObject?.Invoke(obj);
    }
}
