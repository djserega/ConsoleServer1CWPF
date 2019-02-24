using System.Collections.Generic;

namespace ConsoleServer1C.Models
{
    internal static class ListNoAccessBase
    {
        internal static List<InfoBase> List { get; set; } = new List<InfoBase>();
        internal static List<string> ListName { get; set; } = new List<string>();
    }
}
