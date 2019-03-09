using System.Collections.Generic;

namespace ConsoleServer1C.Models
{
    /// <summary>
    /// Базовый класс описания элемента базы данных
    /// </summary>
    public class InfoBase : NotifyPropertyChangedClass
    {
        /// <summary>
        /// Имя базы данных
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Имя базы данных в верхнем регистре
        /// </summary>
        public string NameToUpper { get => Name.ToUpper(); }

        /// <summary>
        /// Описание базы данных
        /// </summary>
        public string Descr { get; set; } = string.Empty;

        /// <summary>
        /// Количество подключений
        /// </summary>
        public int ConnectionCount { get; set; }

        /// <summary>
        /// Количество сессий
        /// </summary>
        public int SessionCount { get; set; }

        /// <summary>
        /// Строка описания времени захвата СУБД
        /// </summary>
        public string DbProcInfo { get; set; } = string.Empty;

        /// <summary>
        /// Количество миллисекунд времени захвата СУБД
        /// </summary>
        public float DbProcTook { get; set; }

        /// <summary>
        /// Есть доступ авторизованного пользователя к базе данных
        /// </summary>
        public bool HaveAccess { get; set; }

        /// <summary>
        /// Список сессий базы данных
        /// </summary>
        public List<Session> ListSessions { get; set; } = new List<Session>();

        /// <summary>
        /// Заполнение объекта с копии
        /// </summary>
        /// <param name="infoBase"></param>
        internal void Fill(InfoBase infoBase)
        {
            Name = infoBase.Name;
            Descr = infoBase.Descr;
            ConnectionCount = infoBase.ConnectionCount;
            SessionCount = infoBase.SessionCount;
            HaveAccess = infoBase.HaveAccess;
            DbProcInfo = infoBase.DbProcInfo;
            DbProcTook = infoBase.DbProcTook;
        }

        /// <summary>
        /// Очистка данных сессий
        /// </summary>
        internal void ClearSessionInfo()
        {
            SessionCount = 0;
            DbProcInfo = string.Empty;
            DbProcTook = 0;
            ListSessions.Clear();
        }

        /// <summary>
        /// Переопределение ToString() для вывода наименования базы данных
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
