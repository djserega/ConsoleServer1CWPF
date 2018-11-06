using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V83;

namespace ConsoleServer1C
{
    internal class ConnectToAgent : IDisposable
    {
        private readonly string _serverName;
        private COMConnector _comConnector;
        private IServerAgentConnection _serverAgent;
        private string _filterInfoBaseName = string.Empty;
        private List<string> _listFilterInfoBaseName = new List<string>();

        internal ConnectToAgent(string serverName)
        {
            _serverName = serverName;
        }

        internal List<Models.InfoBase> InfoBases { get; set; } = new List<Models.InfoBase>();
        internal bool UpdateSessions { get; set; }
        internal string FilterInfoBaseName
        {
            get => _filterInfoBaseName;
            set
            {
                _filterInfoBaseName = value ?? string.Empty;
                _listFilterInfoBaseName.Clear();
                foreach (string item in _filterInfoBaseName.Split(';'))
                {
                    string partNameInfoBase = item.Trim();
                    if (!string.IsNullOrWhiteSpace(partNameInfoBase))
                        _listFilterInfoBaseName.Add(partNameInfoBase.ToUpper());
                }
            }
        }

        public void Dispose()
        {
            _serverAgent = null;
            _comConnector = null;
        }

        internal async Task GetListBaseAsync()
        {
            await Task.Run(() => GetListBaseFromComAsync());
        }

        private async void GetListBaseFromComAsync()
        {
            InitializeComConnector();

            Models.ListNoAccessBase.List.Clear();

            Events.ConnectionStatusEvents.ClearState();

            if (UpdateSessions)
                for (int i = 0; i < InfoBases.Count; i++)
                    InfoBases[i].ClearSessionInfo();

            await FillInfoBasesAllClusters();

            Events.UpdateInfoMainWindowEvents.InfoBases = InfoBases;
            Events.UpdateInfoMainWindowEvents.EvokeUpdateListBasesMainWindow();
        }

        private async Task FillInfoBasesAllClusters()
        {
            Array arrayClusters = _serverAgent.GetClusters();

            Events.ConnectionStatusEvents.CountClusters = arrayClusters.Length;

            foreach (IClusterInfo clusterInfo in arrayClusters)
            {
                _serverAgent.Authenticate(clusterInfo, "", "");

                if (!UpdateSessions)
                {
                    List<Models.InfoBase> infoBasesCluster = await Task.Run(() => GetListInfoBaseFromClusterInfo(_comConnector, _serverAgent, clusterInfo));

                    foreach (Models.InfoBase item in infoBasesCluster)
                        InfoBases.Add(item);
                }

                if (_serverAgent == null)
                {
                    InitializeComConnector();
                    _serverAgent.Authenticate(clusterInfo, "", "");
                }

                GetInfoSessions(_serverAgent, clusterInfo);

                Events.ConnectionStatusEvents.CurrentCluster++;
            }
        }

        private async Task<List<Models.InfoBase>> GetListInfoBaseFromClusterInfo(COMConnector comConnector, IServerAgentConnection serverAgent, IClusterInfo clusterInfo)
        {
            List<Models.InfoBase> infoBasesCluster = new List<Models.InfoBase>();

            Array workingProcesses = null;

            try
            {
                workingProcesses = serverAgent.GetWorkingProcesses(clusterInfo);
            }
            catch (Exception ex)
            {
                return infoBasesCluster;
            }

            if (workingProcesses == null)
                return infoBasesCluster;

            List<Task> tasks = new List<Task>();

            Events.ConnectionStatusEvents.CountWorkProcesses += workingProcesses.Length;

            foreach (IWorkingProcessInfo workProcess in workingProcesses)
                tasks.Add(FillInfoBaseFromWorkProcessAsync(comConnector, workProcess));

            await Task.WhenAll(tasks);

            foreach (Task<List<Models.InfoBase>> item in tasks)
            {
                foreach (Models.InfoBase itemResult in item.Result)
                {
                    if (itemResult != null)
                    {
                        Models.InfoBase infoBase = infoBasesCluster.FirstOrDefault(f => f.NameToUpper == itemResult.NameToUpper);
                        if (infoBase == null)
                            infoBasesCluster.Add(itemResult);
                        else
                        {
                            infoBase.ConnectionCount += itemResult.ConnectionCount;
                            infoBase.HaveAccess = itemResult.HaveAccess;
                        }
                    }
                }
            }

            return infoBasesCluster;
        }

        private async Task<List<Models.InfoBase>> FillInfoBaseFromWorkProcessAsync(COMConnector comConnector, IWorkingProcessInfo workProcess)
        {
            return await Task.Run(() => FillInfoBaseFromWorkProcess(GetWorkingProcessConnection(comConnector, workProcess)));
        }

        private List<Models.InfoBase> FillInfoBaseFromWorkProcess(IWorkingProcessConnection workingProcessConnection)
        {
            List<Models.InfoBase> listInfoBasesTask = new List<Models.InfoBase>();

            Array infoBases = workingProcessConnection.GetInfoBases();

            Events.ConnectionStatusEvents.CountInfoBases += infoBases.Length;

            foreach (IInfoBaseInfo infoBaseInfo in infoBases)
            {
                if (CurrentInfoBaseNameContainedInListFilter(infoBaseInfo.Name.ToUpper()))
                {
                    IInfoBaseConnectionInfo infoBaseConnectionComConsole = FillInfoBase(workingProcessConnection, infoBaseInfo, listInfoBasesTask);
                    if (infoBaseConnectionComConsole != null)
                        workingProcessConnection.Disconnect(infoBaseConnectionComConsole);
                }
                Events.ConnectionStatusEvents.CurrentInfoBases++;
            }

            Events.ConnectionStatusEvents.CurrentWorkProcesses++;

            return listInfoBasesTask;
        }

        private IInfoBaseConnectionInfo FillInfoBase(IWorkingProcessConnection workingProcessConnection, IInfoBaseInfo infoBaseInfo, List<Models.InfoBase> listInfoBasesTask)
        {
            if (workingProcessConnection == null)
                return null;

            if (InfoBaseWithoutAccess.InfoBaseContains(infoBaseInfo.DBName))
                return null;

            IInfoBaseConnectionInfo infoBaseConnectionComConsole = null;
            bool haveAccess = true;
            int connections = 0;
            try
            {
                foreach (IInfoBaseConnectionInfo infoBaseConnectionInfo in workingProcessConnection.GetInfoBaseConnections(infoBaseInfo))
                    if (infoBaseConnectionInfo.AppID == "COMConsole")
                        infoBaseConnectionComConsole = infoBaseConnectionInfo;
                    else if (infoBaseConnectionInfo.AppID != "SrvrConsole")
                        connections++;
            }
            catch (Exception)
            {
                haveAccess = false;
            }

            Models.InfoBase infoBase = listInfoBasesTask.FirstOrDefault(f => f.NameToUpper == infoBaseInfo.Name.ToUpper());
            if (infoBase == null)
            {
                infoBase = new Models.InfoBase()
                {
                    Name = infoBaseInfo.Name,
                    Descr = infoBaseInfo.Descr,
                    HaveAccess = haveAccess
                };
                infoBase.ConnectionCount += connections;
                listInfoBasesTask.Add(infoBase);
            }
            else
            {
                infoBase.ConnectionCount += connections;
                infoBase.HaveAccess = haveAccess;
            }

            if (!haveAccess)
                InfoBaseWithoutAccess.AddInfoBase(infoBaseInfo.DBName);

            return infoBaseConnectionComConsole;
        }

        private void GetInfoSessions(IServerAgentConnection serverAgent, IClusterInfo clusterInfo)
        {
            Array sessions = serverAgent.GetSessions(clusterInfo);
            foreach (ISessionInfo sessionInfo in sessions)
            {
                if (CurrentInfoBaseNameContainedInListFilter(sessionInfo.infoBase.Name.ToUpper()))
                {
                    if (sessionInfo.AppID != "COMConsole"
                        && sessionInfo.AppID != "SrvrConsole")
                    {
                        Models.InfoBase infoBase = InfoBases.FirstOrDefault(f => f.NameToUpper == sessionInfo.infoBase.Name.ToUpper());
                        if (infoBase != null)
                        {
                            infoBase.SessionCount++;
                            infoBase.DbProcTook += ((float)sessionInfo.dbProcTook / 1000);
                            infoBase.DbProcInfo += sessionInfo.dbProcInfo;
                            infoBase.ListSessions.Add(new Models.Session(clusterInfo, sessionInfo));
                        }
                    }
                }
            }
        }

        private IWorkingProcessConnection GetWorkingProcessConnection(COMConnector comConnector, IWorkingProcessInfo workProcess)
        {
            IWorkingProcessConnection workingProcessConnection;
            try
            {
                workingProcessConnection = comConnector.ConnectWorkingProcess($"{_serverName}:{workProcess.MainPort}");
            }
            catch (Exception)
            {
                return null;
            }

            AddAuthentificationWorkingProcess(workingProcessConnection);

            return workingProcessConnection;
        }

        private static void AddAuthentificationWorkingProcess(IWorkingProcessConnection workingProcessConnection)
        {
            workingProcessConnection.AddAuthentication("", "");
        }

        private void InitializeComConnector()
        {
            if (string.IsNullOrWhiteSpace(_serverName))
            {
                throw new ArgumentException("Не указано имя сервера.");
            }

            try
            {
                _comConnector = new COMConnector();
            }
            catch (Exception ex)
            {
                throw new CreateV83ComConnector(ex.Message);
            }

            try
            {
                _serverAgent = _comConnector.ConnectAgent(_serverName);
            }
            catch (Exception ex)
            {
                throw new ConnectAgentException(ex.Message);
            }
        }

        private bool CurrentInfoBaseNameContainedInListFilter(string infoBaseName)
        {
            if (_listFilterInfoBaseName.Count > 0)
            {
                foreach (string filter in _listFilterInfoBaseName)
                {
                    if (infoBaseName.Contains(filter))
                        return true;
                }
                return false;
            }
            else
                return true;
        }

        internal void TerminateSession(Models.Session session)
        {
            if (session == null)
                return;

            InitializeComConnector();

            try
            {
                foreach (IClusterInfo itemClusterInfo in _serverAgent.GetClusters())
                {
                    _serverAgent.Authenticate(itemClusterInfo, "", "");

                    // Terminate
                    foreach (ISessionInfo itemSessionInfo in _serverAgent.GetSessions(itemClusterInfo))
                    {
                        if (itemSessionInfo.infoBase.Name == session.InfoBaseShort.Name
                            && itemSessionInfo.SessionID == session.SessionID)
                        {
                            _serverAgent.TerminateSession(itemClusterInfo, itemSessionInfo);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new TerminateSessionException(ex.Message);
            }
        }
    }
}
