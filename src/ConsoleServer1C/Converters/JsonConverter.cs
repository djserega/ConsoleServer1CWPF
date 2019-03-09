using Newtonsoft.Json;

namespace ConsoleServer1C.Converters
{
    /// <summary>
    /// Сериализация/Десериализация значения в/с JSON
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class JsonConverter<T> where T : class
    {
        /// <summary>
        /// Object (T) в JSON-string
        /// </summary>
        /// <param name="obj">Объект который необходимо сериализовать</param>
        /// <returns>Строка-результат</returns>
        public static string Save(T obj)
            => JsonConvert.SerializeObject(obj);

        /// <summary>
        /// JSON-string в object (T)
        /// </summary>
        /// <param name="text">Строка с данными в формате JSON</param>
        /// <returns>Результат десериализации</returns>
        public static T Load(string text)
            => JsonConvert.DeserializeObject<T>(text) as T;
    }
}
