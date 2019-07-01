using ConsoleServer1C.TaskbarIcon.Events;
using System;
using V83;

namespace ConsoleServer1C.Connector.Models
{
    /// <summary>
    /// Базовый класс данных сессии базы данных
    /// </summary>
    public class Session
    {
        private float _dbProcTook;

        /// <summary>
        /// Конструктор создания сессии
        /// </summary>
        /// <param name="clusterInfo">Данные кластера</param>
        /// <param name="sessionInfo">Данные интерфейса сессии</param>
        public Session(IClusterInfo clusterInfo, ISessionInfo sessionInfo)
        {
            if (clusterInfo == null || sessionInfo == null)
                return;

            ClusterInfo = clusterInfo;
            InfoBaseShort = sessionInfo.infoBase;
            SessionInfo = sessionInfo;
            AppID = sessionInfo.AppID;
            SessionID = sessionInfo.SessionID;
            UserName = sessionInfo.userName;
            Process = sessionInfo.process;
            Connection = sessionInfo.connection;
            StartedAt = sessionInfo.StartedAt;
            ConnID = Connection == null ? 0 : Connection.ConnID;
            DbmsBytesLast5Min = sessionInfo.dbmsBytesLast5Min;
            MemoryLast5Min = sessionInfo.MemoryLast5Min;
            DbProcTook = ((float)sessionInfo.dbProcTook / 1000);
            Host = sessionInfo.Host;
        }

        /// <summary>
        /// Кластер сессии
        /// </summary>
        public IClusterInfo ClusterInfo { get; private set; }

        /// <summary>
        /// Краткая информация о базе данных сессии
        /// </summary>
        public IInfoBaseShort InfoBaseShort { get; private set; }

        /// <summary>
        /// Источник данных сессии
        /// </summary>
        public ISessionInfo SessionInfo { get; private set; }

        /// <summary>
        /// Идентификатор приложения
        /// </summary>
        public string AppID { get; private set; }

        /// <summary>
        /// Идентификатор сессии
        /// </summary>
        public int SessionID { get; private set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Данные рабочего процесса
        /// </summary>
        public IWorkingProcessInfo Process { get; private set; }

        /// <summary>
        /// Краткая информация соединения 
        /// </summary>
        public IConnectionShort Connection { get; private set; }

        /// <summary>
        /// Время захвата СУБД
        /// </summary>
        public float DbProcTook
        {
            get => _dbProcTook;
            private set
            {
                _dbProcTook = value;
            }
        }

        /// <summary>
        /// Дата, время старта сессии
        /// </summary>
        public DateTime StartedAt { get; private set; }

        /// <summary>
        /// Обработано байт за последние 5 минут
        /// </summary>
        public ulong DbmsBytesLast5Min { get; private set; }

        /// <summary>
        /// Обработано байт за последние 5 минут (конвертировано)
        /// </summary>
        public string DbmsBytesLast5MinString { get => Converters.DateConverters.BytesToString(DbmsBytesLast5Min); }

        /// <summary>
        /// Использовано памяти за последние 5 минут
        /// </summary>
        public long MemoryLast5Min { get; private set; }

        /// <summary>
        /// Использовано памяти за последние 5 минут (конвертировано)
        /// </summary>
        public string MemoryLast5MinString { get => Converters.DateConverters.BytesToString(MemoryLast5Min); }

        /// <summary>
        /// Идентификатор соединения
        /// </summary>
        public int ConnID { get; private set; }

        /// <summary>
        /// Имя хоста
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Заполнение по данным с другой сессии
        /// </summary>
        /// <param name="session"></param>
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
            Host = session.Host;
        }

        /// <summary>
        /// Отображение уведомления для пользователя о превышении времени захвата СУБД
        /// </summary>
        public void ShowNotifyDbProcTook(float critical, float high)
        {
            if (_dbProcTook > critical)
            {
                ShowNotify(
                    $"Превышен порог времени соединения с СУБД: {high}",
                    $"Пользователь: {UserName}.\n" +
                    $"Компьютер: {Host}.\n" +
                    $"Время блокировки: {DbProcTook}.\n" +
                    $"Начало сеанса: {StartedAt}.");
            }
        }

        /// <summary>
        /// Вывод уведомления пользователю
        /// </summary>
        /// <param name="title">Заголовок</param>
        /// <param name="message">Текст сообщения</param>
        private void ShowNotify(string title, string message)
        {
            TaskbarIconEvents.ShowTaskbarIcon(title, message);
        }

    }
}
