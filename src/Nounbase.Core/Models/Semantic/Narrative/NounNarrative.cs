using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace Nounbase.Core.Models.Semantic.Narrative
{
    public class NounNarrative
    {
        [JsonProperty("what_is")]
        [JsonPropertyName("what_is")]
        public string? WhatIs { get; set; }

        [JsonProperty("interact_with_other")]
        [JsonPropertyName("interact_with_other")]
        public InteractionNarrative? HowDoesItInteractWith { get; set; }

        public string ToPromptSection(string noun) =>
            $$"""
              WHAT IS A {{noun.ToUpper()}}?

              {{WhatIs}}

              HOW DOES A {{noun.ToUpper()}} INTERACT WITH OTHER PEOPLE IN THIS DOMAIN?

              {{HowDoesItInteractWith?.OtherPeople}}

              HOW DOES A {{noun.ToUpper()}} INTERACT WITH OTHER PLACES IN THIS DOMAIN?
    
              {{HowDoesItInteractWith?.OtherPlaces}}

              HOW DOES A {{noun.ToUpper()}} INTERACT WITH OTHER THINGS IN THIS DOMAIN?
        
              {{HowDoesItInteractWith?.OtherThings}}
              """;
    }
}
