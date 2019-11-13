using ConsoleServer1C.Connector.Models;
using ConsoleServer1C.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleServer1C.Connector
{
    /// <summary>
    /// Класс подключения к серверу 1С
    /// </summary>
    public sealed class ConnectToAgent : IDisposable
    {
        #region Private fields

        /// <summary>
        /// Имя сервера
        /// </summary>
        private readonly string _serverName;
        /// <summary>
        /// Объект подключения
        /// </summary>
        //private COMConnector _comConnector;
        private dynamic _comConnector;
        /// <summary>
        /// Данные подключения к агенту сервера 1С
        /// </summary>
        //private IServerAgentConnection _serverAgent;
        private dynamic _serverAgent;
        /// <summary>
        /// Список отбора баз
        /// </summary>
        private string _filterInfoBaseName = string.Empty;
        /// <summary>
        /// Список имен баз данных разделенных по разделителю (_filterInfoBaseName)
        /// </summary>
        private readonly List<string> _listFilterInfoBaseName = new List<string>();
        /// <summary>
        /// Параметры подключения к рабочему процессу
        /// </summary>
        private static string[] _connectSettings;

        #endregion

        /// <summary>
        /// Базовый конструктор класса
        /// </summary>
        /// <param name="serverName"></param>
        public ConnectToAgent(string serverName)
        {
            _serverName = serverName;
        }

        /// <summary>
        /// Конструктор класса с параметрами подключения
        /// </summary>
        /// <param name="serverName"></param>
        public ConnectToAgent(string serverName, string[] connectSettings) : this(serverName)
        {
            _connectSettings = connectSettings;
        }

        /// <summary>
        /// Список баз данных
        /// </summary>
        public List<InfoBase> InfoBases { get; set; } = new List<InfoBase>();

        /// <summary>
        /// Признак обновления только данных сессий
        /// </summary>
        public bool UpdateSessions { get; set; }

        /// <summary>
        /// Список баз данных полученных в результате разбиения строки по разделителю
        /// </summary>
        public string FilterInfoBaseName
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

        /// <summary>
        /// Деструктор
        /// </summary>
        public void Dispose()
        {
            _serverAgent = null;
            _comConnector = null;
        }

        /// <summary>
        /// Получение списка баз данных (асинхронно)
        /// </summary>
        /// <returns></returns>
        public async Task GetListBaseAsync()
        {
            await Task.Run(() => GetListBaseFromComAsync());
        }

        /// <summary>
        /// Отключение сессии
        /// </summary>
        /// <param name="session"></param>
        public void TerminateSession(Models.Session session)
        {
            if (session == null)
                return;

            InitializeComConnector();

            try
            {
                //foreach (IClusterInfo itemClusterInfo in _serverAgent.GetClusters())
                foreach (dynamic itemClusterInfo in _serverAgent.GetClusters())
                {
                    _serverAgent.Authenticate(itemClusterInfo, "", "");

                    // Terminate
                    //foreach (ISessionInfo itemSessionInfo in _serverAgent.GetSessions(itemClusterInfo))
                    foreach (dynamic itemSessionInfo in _serverAgent.GetSessions(itemClusterInfo))
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

        #region Private methods

        /// <summary>
        /// Получение списка баз данных с COMОбъекта (асинхронно)
        /// </summary>
        private async void GetListBaseFromComAsync()
        {
            Models.ListNoAccessBase.List.Clear();

            Events.ConnectionStatusEvents.ClearState();

            try
            {
                if (CheckConnectionServer.CheckConnectionToServer(_serverName))
                {
                    InitializeComConnector();

                    if (UpdateSessions)
                        for (int i = 0; i < InfoBases.Count; i++)
                            InfoBases[i].ClearSessionInfo();

                    await FillInfoBasesAllClusters();
                }
            }
            catch (ArgumentException ex)
            {
                Dialogs.Show(ex.Message);
            }
            catch (CreateV83ComConnector ex)
            {
                Dialogs.Show($"Не удалось создать COMConnector.\n{ex.Message}");
            }
            catch (ConnectAgentException ex)
            {
                Dialogs.Show($"Ошибка соединения с сервером.\n{ex.Message}");
            }
            catch (WorkingProcessException ex)
            {
                Dialogs.Show($"Ошибка соединения с рабочим процессом.\n{ex.Message}");
            }
            catch (Exception ex)
            {
                Dialogs.Show("Не обработанная ошибка получения списка баз: \n" + ex.Message);
                Dialogs.Show(ex.StackTrace);
            }

            Events.UpdateInfoMainWindowEvents.InfoBases = InfoBases;
            Events.UpdateInfoMainWindowEvents.EvokeUpdateListBasesMainWindow(UpdateSessions);
        }

        /// <summary>
        /// Заполнение списка информационных баз по данным кластеров сервера 1С
        /// </summary>
        /// <returns></returns>
        private async Task FillInfoBasesAllClusters()
        {
            Array arrayClusters = _serverAgent.GetClusters();

            Events.ConnectionStatusEvents.CountClusters = arrayClusters.Length;

            //foreach (IClusterInfo clusterInfo in arrayClusters)
            foreach (dynamic clusterInfo in arrayClusters)
            {
                _serverAgent.Authenticate(clusterInfo, "", "");

                if (!UpdateSessions)
                {
                    //List<Models.InfoBase> infoBasesCluster = await Task.Run(() => GetListInfoBaseFromClusterInfo(_comConnector, _serverAgent, clusterInfo)).Result;
                    List<Models.InfoBase> infoBasesCluster = GetListInfoBaseFromClusterInfo(_comConnector, _serverAgent, clusterInfo).Result;

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

        /// <summary>
        /// Получение списка баз данных по кластеру
        /// </summary>
        /// <param name="comConnector">COM объект подключения к серверу 1С</param>
        /// <param name="serverAgent">Подключение к агенту сервера 1С</param>
        /// <param name="clusterInfo">Кластер со списком баз данных</param>
        /// <returns>Задача с результатом: список баз данных</returns>
        //private async Task<List<Models.InfoBase>> GetListInfoBaseFromClusterInfo(COMConnector comConnector, IServerAgentConnection serverAgent, IClusterInfo clusterInfo)
        private async Task<List<Models.InfoBase>> GetListInfoBaseFromClusterInfo(dynamic comConnector, dynamic serverAgent, dynamic clusterInfo)
        {
            List<Models.InfoBase> infoBasesCluster = new List<Models.InfoBase>();

            Array workingProcesses = null;

            try
            {
                workingProcesses = serverAgent.GetWorkingProcesses(clusterInfo);
            }
            catch (Exception)
            {
                return infoBasesCluster;
            }

            if (workingProcesses == null)
                return infoBasesCluster;

            List<Task> tasks = new List<Task>();

            Events.ConnectionStatusEvents.CountWorkProcesses += workingProcesses.Length;

            //foreach (IWorkingProcessInfo workProcess in workingProcesses)
            foreach (dynamic workProcess in workingProcesses)
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

        /// <summary>
        /// Получение списка информационных баз рабочего процесса (асинхронно)
        /// </summary>
        /// <param name="comConnector">COM объект подключения к серверу 1С</param>
        /// <param name="workProcess">Рабочий процесс</param>
        /// <returns>Задача с результатом: Список баз данных</returns>
        //private async Task<List<Models.InfoBase>> FillInfoBaseFromWorkProcessAsync(COMConnector comConnector, IWorkingProcessInfo workProcess)
        private async Task<List<Models.InfoBase>> FillInfoBaseFromWorkProcessAsync(dynamic comConnector, dynamic workProcess)
        {
            return await Task.Run(() => FillInfoBaseFromWorkProcess(GetWorkingProcessConnection(comConnector, workProcess)));
        }

        /// <summary>
        /// Получение списка информационных баз рабочего процесса
        /// </summary>
        /// <param name="workingProcessConnection"></param>
        /// <returns>Список баз данных</returns>
        //private List<Models.InfoBase> FillInfoBaseFromWorkProcess(IWorkingProcessConnection workingProcessConnection)
        private List<Models.InfoBase> FillInfoBaseFromWorkProcess(dynamic workingProcessConnection)
        {
            List<Models.InfoBase> listInfoBasesTask = new List<Models.InfoBase>();

            if (workingProcessConnection == null)
                return listInfoBasesTask;

            Array infoBases = workingProcessConnection.GetInfoBases();

            Events.ConnectionStatusEvents.CountInfoBases += infoBases.Length;

            //foreach (IInfoBaseInfo infoBaseInfo in infoBases)
            foreach (dynamic infoBaseInfo in infoBases)
            {
                if (CurrentInfoBaseNameContainedInListFilter(infoBaseInfo.Name.ToUpper()))
                {
                    //IInfoBaseConnectionInfo infoBaseConnectionComConsole = FillInfoBase(workingProcessConnection, infoBaseInfo, listInfoBasesTask);
                    dynamic infoBaseConnectionComConsole = FillInfoBase(workingProcessConnection, infoBaseInfo, listInfoBasesTask);
                    if (infoBaseConnectionComConsole != null)
                    {
                        try
                        {
                            workingProcessConnection.Disconnect(infoBaseConnectionComConsole);
                        }
                        catch (Exception) { }
                    }
                }
                Events.ConnectionStatusEvents.CurrentInfoBases++;
            }

            Events.ConnectionStatusEvents.CurrentWorkProcesses++;

            return listInfoBasesTask;
        }

        /// <summary>
        /// Обновление данных информационной базы данных
        /// </summary>
        /// <param name="workingProcessConnection">Подключение к рабочему процессу</param>
        /// <param name="infoBaseInfo">Информация о базе данных</param>
        /// <param name="listInfoBasesTask">Список баз данных</param>
        /// <returns>Информация о подключении COMConsole</returns>
        //private IInfoBaseConnectionInfo FillInfoBase(IWorkingProcessConnection workingProcessConnection, IInfoBaseInfo infoBaseInfo, List<Models.InfoBase> listInfoBasesTask)
        private dynamic FillInfoBase(dynamic workingProcessConnection, dynamic infoBaseInfo, List<Models.InfoBase> listInfoBasesTask)
        {
            if (workingProcessConnection == null)
                return null;

            if (InfoBaseWithoutAccess.InfoBaseContains(infoBaseInfo.DBName))
                return null;

            //IInfoBaseConnectionInfo infoBaseConnectionComConsole = null;
            dynamic infoBaseConnectionComConsole = null;
            bool haveAccess = true;
            int connections = 0;
            try
            {
                //foreach (IInfoBaseConnectionInfo infoBaseConnectionInfo in workingProcessConnection.GetInfoBaseConnections(infoBaseInfo))
                foreach (dynamic infoBaseConnectionInfo in workingProcessConnection.GetInfoBaseConnections(infoBaseInfo))
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

        /// <summary>
        /// Получение информации о сессии
        /// </summary>
        /// <param name="serverAgent">Подключение к агенту сервера 1С</param>
        /// <param name="clusterInfo">Данные кластера 1С</param>
        //private void GetInfoSessions(IServerAgentConnection serverAgent, IClusterInfo clusterInfo)
        private void GetInfoSessions(dynamic serverAgent, dynamic clusterInfo)
        {
            Array sessions = serverAgent.GetSessions(clusterInfo);
            //foreach (ISessionInfo sessionInfo in sessions)
            foreach (dynamic sessionInfo in sessions)
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

        /// <summary>
        /// Получение списка рабочих процессов
        /// </summary>
        /// <param name="comConnector">COM объект подключения к серверу 1С</param>
        /// <param name="workProcess">Рабочий процесс</param>
        /// <returns></returns>
        //private IWorkingProcessConnection GetWorkingProcessConnection(COMConnector comConnector, IWorkingProcessInfo workProcess)
        private dynamic GetWorkingProcessConnection(dynamic comConnector, dynamic workProcess)
        {
            //IWorkingProcessConnection workingProcessConnection;
            dynamic workingProcessConnection;
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

        /// <summary>
        /// Аутентификация к рабочему процессу
        /// </summary>
        /// <param name="workingProcessConnection">Рабочий процесс</param>
        //private static void AddAuthentificationWorkingProcess(IWorkingProcessConnection workingProcessConnection)
        private static void AddAuthentificationWorkingProcess(dynamic workingProcessConnection)
        {
            if (_connectSettings == null)
                workingProcessConnection.AddAuthentication("", "");
            else
                workingProcessConnection.AddAuthentication(
                    AppSettings.ConverterInValue(_connectSettings[0]),
                    AppSettings.ConverterInValue(_connectSettings[1]));
        }

        /// <summary>
        /// Инициализация объекта подключения
        /// </summary>
        private void InitializeComConnector()
        {
            if (string.IsNullOrWhiteSpace(_serverName))
            {
                throw new ArgumentException("Не указано имя сервера.");
            }

            try
            {
                //_comConnector = new COMConnector();
                Type typeConnector = Type.GetTypeFromProgID("V83.COMConnector");
                if (typeConnector == null)
                    throw new Exception("Не удалось создать тип V83.COMConnector.");

                _comConnector = Activator.CreateInstance(typeConnector);
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

        /// <summary>
        /// Проверка наличия базы данных в списке отборов
        /// </summary>
        /// <param name="infoBaseName">Имя базы данных</param>
        /// <returns>true: имя базы данных подходит под отбор (или отбор пустой)</returns>
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

        #endregion

    }
}
