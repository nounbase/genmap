using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Schema
{
    public class Query
    {
        public Query() { }

        public Query(string title, string queryText)
        {
            Title = title;
            QueryText = queryText;
        }

        [JsonProperty("title")]
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonProperty("query")]
        [JsonPropertyName("query")]
        public string? QueryText { get; set; }
    }
}
