using System;
using System.Collections.Generic;

namespace ConsoleServer1C.Events
{
    /// <summary>
    /// Делегат обновления данных результата подключения
    /// </summary>
    /// <param name="updateSessions">Признак обновления данных сессий</param>
    internal delegate void UpdateListBasesMainWindowEvent(bool updateSessions);

    /// <summary>
    /// Класс обновления данных подключения к серверу 1С
    /// </summary>
    internal class UpdateInfoMainWindowEvents : EventArgs
    {
        /// <summary>
        /// Список полученных баз данных
        /// </summary>
        internal static List<Models.InfoBase> InfoBases { get; set; }

        /// <summary>
        /// Событие изменения списка баз данных
        /// </summary>
        internal static event UpdateListBasesMainWindowEvent UpdateListBasesMainWindowEvent;

        /// <summary>
        /// Метод обновления данных результата подключения
        /// </summary>
        /// <param name="updateSessions">Признак обновления данных сессий</param>
        internal static void EvokeUpdateListBasesMainWindow(bool updateSessions)
            => UpdateListBasesMainWindowEvent?.Invoke(updateSessions);
    }
}
