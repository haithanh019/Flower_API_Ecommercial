using System.Text.Json.Serialization;

namespace DataAccess.Helper
{
    public class OWrapper<T>
    {
        [JsonPropertyName("value")]
        public List<T> Value { get; set; } = [];

        [JsonPropertyName("@odata.count")]
        public int Count { get; set; }
    }
}
