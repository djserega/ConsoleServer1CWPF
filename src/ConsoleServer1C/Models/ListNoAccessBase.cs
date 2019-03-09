using System.Collections.Generic;

namespace ConsoleServer1C.Models
{
    /// <summary>
    /// Список баз данных без доступа
    /// </summary>
    internal static class ListNoAccessBase
    {
        /// <summary>
        /// Список элементов InfoBase
        /// </summary>
        internal static List<InfoBase> List { get; set; } = new List<InfoBase>();

        /// <summary>
        /// Список элементов по имени базы данных
        /// </summary>
        internal static List<string> ListName { get; set; } = new List<string>();
    }
}
