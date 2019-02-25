using System;

namespace ConsoleServer1C.Models
{
    public class HistoryConnection
    {
        private HistoryConnection()
        {
        }

        public HistoryConnection(string server, string filterBase) : this()
        {
            Server = server;
            FilterBase = filterBase;
        }

        public HistoryConnection(string server, string filterBase, bool setDate) : this(server, filterBase)
        {
            if (setDate)
                Date = DateTime.Now;
        }

        public DateTime Date { get; set; }
        public string Header { get => $"{Server} \\ {FilterBase}"; }
        public string ToolTip { get => Date.ToString("dd.MM.yyyy HH:mm:ss"); }
        public string Server { get; set; } = string.Empty;
        public string FilterBase { get; set; } = string.Empty;
    }
}
