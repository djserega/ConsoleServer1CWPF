using System;
using System.Collections.Generic;

namespace ConsoleServer1C.Connector.Events
{
    /// <summary>
    /// Делегат обновления данных результата подключения
    /// </summary>
    /// <param name="updateSessions">Признак обновления данных сессий</param>
    public delegate void UpdateListBasesMainWindowEvent(bool updateSessions);

    /// <summary>
    /// Класс обновления данных подключения к серверу 1С
    /// </summary>
    public class UpdateInfoMainWindowEvents : EventArgs
    {
        /// <summary>
        /// Список полученных баз данных
        /// </summary>
        public static List<Models.InfoBase> InfoBases { get; set; }

        /// <summary>
        /// Событие изменения списка баз данных
        /// </summary>
        public static event UpdateListBasesMainWindowEvent UpdateListBasesMainWindowEvent;

        /// <summary>
        /// Метод обновления данных результата подключения
        /// </summary>
        /// <param name="updateSessions">Признак обновления данных сессий</param>
        internal static void EvokeUpdateListBasesMainWindow(bool updateSessions)
            => UpdateListBasesMainWindowEvent?.Invoke(updateSessions);
    }
}
