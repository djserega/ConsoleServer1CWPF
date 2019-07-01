using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ConsoleServer1C.Connector
{
    internal static class Dialogs
    {
        internal static void Show(string text)
        {
            MessageBox.Show(text, "Консоль сервера 1С");
        }
    }
}
