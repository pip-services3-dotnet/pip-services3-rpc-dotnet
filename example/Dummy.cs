using Newtonsoft.Json;
using PipServices3.Commons.Data;
using System;
using System.Collections.Generic;

namespace PipServices3.Rpc
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

        [JsonProperty("param")]
        public DummyParam Param { get; set; }

        [JsonProperty("items")]
        public List<DummyItem> Items { get; set; } = new List<DummyItem>();

        [JsonProperty("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [JsonProperty("date")]
        public DateTime Date { get; set; }
    }

    public class DummyParam
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }

    public class DummyItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
