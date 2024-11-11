using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Semantic.Narrative
{
    public class DomainNarrative
    {
        public DomainNarrative() { }

        public DomainNarrative(string domainDescription) =>
            DomainDescription = domainDescription 
            ?? throw new ArgumentNullException(nameof(domainDescription));

        [JsonProperty("domain_description")]
        [JsonPropertyName("domain_description")]
        public string? DomainDescription { get; set; }
    }
}
