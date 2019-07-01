namespace ConsoleServer1C.TaskbarIcon.Events
{
    /// <summary>
    /// Делегат сообщений на панели уведомлений
    /// </summary>
    /// <param name="title">Заголовок уведомления</param>
    /// <param name="message">Текст сообщения</param>
    public delegate void TaskbarIconEvent(string title, string message);

    /// <summary>
    /// Метод уведомлений
    /// </summary>
    public static class TaskbarIconEvents
    {
        /// <summary>
        /// Событие обновления значения уведомлений
        /// </summary>
        public static event TaskbarIconEvent TaskbarIconEvent;

        /// <summary>
        /// Метод обновления значений на панели уведомлений
        /// </summary>
        /// <param name="title">Заголовок уведомления</param>
        /// <param name="message">Текст сообщения</param>
        public static void ShowTaskbarIcon(string title, string message) => TaskbarIconEvent?.Invoke(title, message);
    }
}
