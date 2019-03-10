using System;
using System.Text;

namespace ConsoleServer1C.Events
{
    public delegate void ConnectionStatusEvent();
    public class ConnectionStatusEvents : EventArgs
    {
        private static int _countClusters = 0;
        private static int _countWorkProcesses = 0;
        private static int _countInfoBases = 0;

        private static int _currentCluster = 0;
        private static int _currentWorkProcesses = 0;
        private static int _currentInfoBases = 0;

        private static int _previousProgress = 0;

        public static int CountClusters         { get => _countClusters;        set { _countClusters        = value; EvokeUpdateState(); } }
        public static int CountWorkProcesses    { get => _countWorkProcesses;   set { _countWorkProcesses   = value; EvokeUpdateState(); } }
        public static int CountInfoBases        { get => _countInfoBases;       set { _countInfoBases       = value; EvokeUpdateState(); } }

        public static int CurrentCluster        { get => _currentCluster;       set { _currentCluster       = value; EvokeUpdateState(); } }
        public static int CurrentWorkProcesses  { get => _currentWorkProcesses; set { _currentWorkProcesses = value; EvokeUpdateState(); } }
        public static int CurrentInfoBases      { get => _currentInfoBases;     set { _currentInfoBases     = value; EvokeUpdateState(); } }

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

        public static event ConnectionStatusEvent ConnectionStatusEvent;
        public static void EvokeUpdateState()
        {
            ConnectionStatusEvent?.Invoke();
        }

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
