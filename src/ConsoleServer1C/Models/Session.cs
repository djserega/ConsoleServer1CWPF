using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V83;

namespace ConsoleServer1C.Models
{
    public class Session : NotifyPropertyChangedClass
    {
        public Session(IClusterInfo clusterInfo, ISessionInfo sessionInfo)
        {
            ClusterInfo = clusterInfo;
            InfoBaseShort = sessionInfo.infoBase;
            SessionInfo = sessionInfo;
            AppID = sessionInfo.AppID;
            SessionID = sessionInfo.SessionID;
            UserName = sessionInfo.userName;
            Process = sessionInfo.process;
            Connection = sessionInfo.connection;
            DbProcTook = ((float)sessionInfo.dbProcTook / 1000);
            StartedAt = sessionInfo.StartedAt;
            ConnID = Connection == null ? 0 : Connection.ConnID;
            DbmsBytesLast5Min = sessionInfo.dbmsBytesLast5Min;
            MemoryLast5Min = sessionInfo.MemoryLast5Min;
        }

        public IClusterInfo ClusterInfo { get; private set; }
        public IInfoBaseShort InfoBaseShort { get; private set; }
        public ISessionInfo SessionInfo { get; private set; }
        public string AppID { get; private set; }
        public int SessionID { get; private set; }
        public string UserName { get; private set; }
        public IWorkingProcessInfo Process { get; private set; }
        public IConnectionShort Connection { get; private set; }
        public float DbProcTook { get; private set; }
        public DateTime StartedAt { get; private set; }
        public ulong DbmsBytesLast5Min { get; private set; }
        public string DbmsBytesLast5MinString { get => Converters.DataConverters.BytesToString(DbmsBytesLast5Min); }
        public long MemoryLast5Min { get; private set; }
        public string MemoryLast5MinString { get => Converters.DataConverters.BytesToString(MemoryLast5Min); }
        public int ConnID { get; private set; }

        internal void Fill(Session session)
        {
            ClusterInfo = session.ClusterInfo;
            InfoBaseShort = session.InfoBaseShort;
            SessionInfo = session.SessionInfo;
            AppID = session.AppID;
            SessionID = session.SessionID;
            UserName = session.UserName;
            Process = session.Process;
            Connection = session.Connection;
            DbProcTook = session.DbProcTook;
            StartedAt = session.StartedAt;
            ConnID = session.ConnID;
            DbmsBytesLast5Min = session.DbmsBytesLast5Min;
            MemoryLast5Min = session.MemoryLast5Min;
        }

    }
}
