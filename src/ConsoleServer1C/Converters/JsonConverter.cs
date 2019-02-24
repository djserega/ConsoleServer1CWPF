using Newtonsoft.Json;

namespace ConsoleServer1C.Converters
{
    public static class JsonConverter<T> where T : class
    {
        public static string Save(T obj)
            => JsonConvert.SerializeObject(obj);

        public static T Load(string text)
            => JsonConvert.DeserializeObject<T>(text) as T;
    }
}
