using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer1C.Models
{
    internal static class ListNoAccessBase
    {
        internal static List<Models.InfoBase> List { get; set; } = new List<Models.InfoBase>();
        internal static List<string> ListName { get; set; } = new List<string>();
    }
}
