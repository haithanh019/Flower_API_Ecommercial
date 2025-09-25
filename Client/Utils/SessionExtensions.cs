using System.Text.Json;

namespace Client.Utils
{
    public static class SessionExtensions
    {
        private static readonly JsonSerializerOptions _opt = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
        };

        public static void SetObject<T>(this ISession session, string key, T value) =>
            session.SetString(key, JsonSerializer.Serialize(value, _opt));

        public static T? GetObject<T>(this ISession session, string key)
        {
            var str = session.GetString(key);
            return string.IsNullOrEmpty(str) ? default : JsonSerializer.Deserialize<T>(str!, _opt);
        }
    }
}
