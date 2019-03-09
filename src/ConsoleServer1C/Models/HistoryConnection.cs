using System;

namespace ConsoleServer1C.Models
{
    /// <summary>
    /// История подключений к серверам 1С
    /// </summary>
    public class HistoryConnection
    {
        /// <summary>
        /// Базовый класс для сериализации/десериализации
        /// </summary>
        private HistoryConnection()
        {
        }

        /// <summary>
        /// Основной класс создания элемента истории
        /// </summary>
        /// <param name="server">Имя сервера</param>
        /// <param name="filterBase">Фильтры списка баз</param>
        public HistoryConnection(string server, string filterBase) : this()
        {
            Server = server;
            FilterBase = filterBase;
        }

        /// <summary>
        /// Класс создания элемента истории с установкой даты создания
        /// </summary>
        /// <param name="server">Имя сервера</param>
        /// <param name="filterBase">Фильтры списка баз</param>
        /// <param name="setDate">Нужно ли установить дату создания элемента истории</param>
        public HistoryConnection(string server, string filterBase, bool setDate) : this(server, filterBase)
        {
            if (setDate)
                Date = DateTime.Now;
        }

        /// <summary>
        /// Дата создания истории
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Представление
        /// </summary>
        public string Header { get => $"{Server} \\ {FilterBase}"; }

        /// <summary>
        /// Подсказка элемента
        /// </summary>
        public string ToolTip { get => Date.ToString("dd.MM.yyyy HH:mm:ss"); }

        /// <summary>
        /// Имя сервера
        /// </summary>
        public string Server { get; set; } = string.Empty;

        /// <summary>
        /// Фильтры списка баз
        /// </summary>
        public string FilterBase { get; set; } = string.Empty;
    }
}
