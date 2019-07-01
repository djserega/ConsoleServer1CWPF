using System.Collections.Generic;

namespace ConsoleServer1C.Connector.Models
{
    /// <summary>
    /// Класс имен баз данных без доступа к информации
    /// </summary>
    public static class InfoBaseWithoutAccess
    {
        /// <summary>
        /// Локер добавления
        /// </summary>
        private static readonly object _lockerAdd = new object();
        /// <summary>
        /// Локер проверки наличия
        /// </summary>
        private static readonly object _lockerContains = new object();

        /// <summary>
        /// Список имен баз данных
        /// </summary>
        public static List<string> InfoBasesName { get; set; } = new List<string>();

        /// <summary>
        /// Добавление базы данных
        /// </summary>
        /// <param name="infoBaseName"></param>
        public static void AddInfoBase(string infoBaseName)
        {
            lock (_lockerAdd)
            {
                if (!InfoBaseContains(infoBaseName))
                    InfoBasesName.Add(infoBaseName);
            }
        }

        /// <summary>
        /// Проверка наличия имени базы данных в списке
        /// </summary>
        /// <param name="infoBaseName"></param>
        /// <returns></returns>
        public static bool InfoBaseContains(string infoBaseName)
        {
            lock (_lockerContains)
            {
                return InfoBasesName.Contains(infoBaseName);
            }
        }
    }
}
