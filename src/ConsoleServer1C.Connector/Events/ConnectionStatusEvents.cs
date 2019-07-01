using System;

namespace ConsoleServer1C.Connector.Events
{
    /// <summary>
    /// Делегат состояния подключения к серверу 1С
    /// </summary>
    public delegate void ConnectionStatusEvent();

    /// <summary>
    /// Класс отображения/расчета текущего состояния подключения к серверу 1С
    /// </summary>
    public class ConnectionStatusEvents : EventArgs
    {
        /// <summary>
        /// Количество кластеров
        /// </summary>
        private static int _countClusters = 0;
        /// <summary>
        /// Количество рабочих процессов
        /// </summary>
        private static int _countWorkProcesses = 0;
        /// <summary>
        /// Количество информационных баз
        /// </summary>
        private static int _countInfoBases = 0;

        /// <summary>
        /// Количество обработанных кластеров
        /// </summary>
        private static int _currentCluster = 0;
        /// <summary>
        /// Количество обработанных рабочих процессов
        /// </summary>
        private static int _currentWorkProcesses = 0;
        /// <summary>
        /// Количество обработанных информационных баз
        /// </summary>
        private static int _currentInfoBases = 0;

        /// <summary>
        /// Предыдущее значение состояния
        /// </summary>
        private static int _previousProgress = 0;

        /// <summary>
        /// Количество кластеров
        /// </summary>
        public static int CountClusters         { get => _countClusters;        set { _countClusters        = value; EvokeUpdateState(); } }
        /// <summary>
        /// Количество рабочих процессов
        /// </summary>
        public static int CountWorkProcesses    { get => _countWorkProcesses;   set { _countWorkProcesses   = value; EvokeUpdateState(); } }
        /// <summary>
        /// Количество информационных баз
        /// </summary>
        public static int CountInfoBases        { get => _countInfoBases;       set { _countInfoBases       = value; EvokeUpdateState(); } }

        /// <summary>
        /// Количество обработанных кластеров
        /// </summary>
        public static int CurrentCluster        { get => _currentCluster;       set { _currentCluster       = value; EvokeUpdateState(); } }
        /// <summary>
        /// Количество обработанных рабочих процессов
        /// </summary>
        public static int CurrentWorkProcesses  { get => _currentWorkProcesses; set { _currentWorkProcesses = value; EvokeUpdateState(); } }
        /// <summary>
        /// Количество обработанных информационных баз
        /// </summary>
        public static int CurrentInfoBases      { get => _currentInfoBases;     set { _currentInfoBases     = value; EvokeUpdateState(); } }

        /// <summary>
        /// Текущее состояние подключения
        /// </summary>
        public static int CurrentStateProgress
        {
            get
            {
                double currentState = _currentCluster + _currentWorkProcesses + _currentInfoBases;
                double allState = _countClusters + _countWorkProcesses + _countInfoBases;

                int progress = (int)(currentState * 100 / allState);

                if (progress < 0)
                    progress = 0;

                if (_previousProgress < progress)
                    _previousProgress = progress;
                else
                    progress = _previousProgress;

                return Math.Min(100, progress);
            }
        }

        /// <summary>
        /// Событие изменения состояния подключения к серверу 1С
        /// </summary>
        public static event ConnectionStatusEvent ConnectionStatusEvent;
        /// <summary>
        /// Метод обновления данных после изменения состояния подключения к серверу 1С
        /// </summary>
        public static void EvokeUpdateState() => ConnectionStatusEvent?.Invoke();

        /// <summary>
        /// Очистка расчетных значений состояния подключения
        /// </summary>
        public static void ClearState()
        {
            _previousProgress = 0;

            _countClusters = 0;
            _countWorkProcesses = 0;
            _countInfoBases = 0;

            _currentCluster = 0;
            _currentWorkProcesses = 0;
            _currentInfoBases = 0;

            EvokeUpdateState();
        }
    }
}
