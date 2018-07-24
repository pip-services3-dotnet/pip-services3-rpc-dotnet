using Newtonsoft.Json;
using PipServices.Commons.Data;

namespace PipServices.Rpc
{
    public class Dummy : IStringIdentifiable
    {
        public Dummy()
        {
        }

        public Dummy(string id, string key, string content, bool flag = true)
        {
            Id = id;
            Key = key;
            Content = content;
            Flag = flag;
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("flag")]
        public bool Flag { get; set; }
    }
}
