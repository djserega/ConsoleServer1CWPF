namespace ConsoleServer1C.Events
{
    public delegate void TaskbarIconEvent(string title, string message);
    public static class TaskbarIconEvents
    {
        public static event TaskbarIconEvent TaskbarIconEvent;
        public static void ShowTaskbarIcon(string title, string message) => TaskbarIconEvent?.Invoke(title, message);
    }
}
