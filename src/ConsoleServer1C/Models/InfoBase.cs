﻿using System.Collections.Generic;

namespace ConsoleServer1C.Models
{
    public class InfoBase : NotifyPropertyChangedClass
    {
        public string Name { get; set; } = string.Empty;
        public string NameToUpper { get => Name.ToUpper(); }
        public string Descr { get; set; } = string.Empty;
        public int ConnectionCount { get; set; }
        public int SessionCount { get; set; }
        public string DbProcInfo { get; set; } = string.Empty;
        public float DbProcTook { get; set; }

        public bool HaveAccess { get; set; }

        public List<Session> ListSessions { get; set; } = new List<Session>();

        internal void Fill(InfoBase infoBase)
        {
            Name = infoBase.Name;
            Descr = infoBase.Descr;
            ConnectionCount = infoBase.ConnectionCount;
            SessionCount = infoBase.SessionCount;
            HaveAccess = infoBase.HaveAccess;
            DbProcInfo = infoBase.DbProcInfo;
            DbProcTook = infoBase.DbProcTook;
        }

        internal void ClearSessionInfo()
        {
            SessionCount = 0;
            DbProcInfo = string.Empty;
            DbProcTook = 0;
            ListSessions.Clear();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
