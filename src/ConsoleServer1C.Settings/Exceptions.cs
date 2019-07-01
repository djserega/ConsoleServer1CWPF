using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer1C.Settings
{
    internal class RegistrykeyNotFoundException : Exception
    {
        internal RegistrykeyNotFoundException(string message) : base(message) { }
    }
}
