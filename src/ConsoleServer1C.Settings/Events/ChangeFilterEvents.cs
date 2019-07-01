namespace ConsoleServer1C.Settings.Events
{
    /// <summary>
    /// Делегат изменения строки фильтра списка баз
    /// </summary>
    public delegate void ChangeFilterFindBaseEvent();

    /// <summary>
    /// Делегат изменения строки фильтра списка сессий
    /// </summary>
    public delegate void ChangeFilterFindUserEvent();

    /// <summary>
    /// Класс фильтрации таблиц списка баз и списка сессий
    /// </summary>
    public class ChangeFilterEvents
    {
        /// <summary>
        /// Событие изменения поля фильтра списка баз
        /// </summary>
        public static event ChangeFilterFindBaseEvent ChangeFilterFindBaseEvent;

        /// <summary>
        /// Метод изменения поля фильтра списка баз
        /// </summary>
        public static void InvokeFindBaseEvent() => ChangeFilterFindBaseEvent?.Invoke();

        /// <summary>
        /// Событие изменения поля фильтра списка сессий
        /// </summary>
        public static event ChangeFilterFindUserEvent ChangeFilterFindUserEvent;

        /// <summary>
        /// Метод изменения поля фильтра списка сессий
        /// </summary>
        public static void InvokeFindUserEvent() => ChangeFilterFindUserEvent?.Invoke();
    }
}
