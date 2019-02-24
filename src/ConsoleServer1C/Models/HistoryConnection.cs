using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer1C.Models
{
    public class HistoryConnection
    {
        private HistoryConnection()
        {
            Date = DateTime.Now;
        }

        public HistoryConnection(string server, string filterBase) : this()
        {
            Server = server;
            FilterBase = filterBase;
        }

        public DateTime Date { get; }
        public string Header { get => $"{Server} \\ {FilterBase}"; }
        public string ToolTip { get => Date.ToString("dd.MM.yyyy HH:mm:ss"); }
        public string Server { get; set; } = string.Empty;
        public string FilterBase { get; set; } = string.Empty;
    }
}
