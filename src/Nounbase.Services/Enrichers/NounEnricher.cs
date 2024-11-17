using Microsoft.Extensions.Logging;
using Nounbase.Core.Extensions;
using Nounbase.Core.Interfaces.Clients;
using Nounbase.Core.Interfaces.Configuration;
using Nounbase.Core.Interfaces.Enrichers;
using Nounbase.Core.Models;
using Nounbase.Core.Models.Schema;
using Nounbase.Core.Models.Semantic;
using System.Text.Json.Serialization;
using static Nounbase.Core.Utilities.Resiliency;

namespace Nounbase.Services.Enrichers
{
    public class NounEnricher : INounEnricher
    {
        private class NounEnrichmentCompletion
        {
            [JsonPropertyName("singular_name")]
            public string? SingularName { get; set; }

            [JsonPropertyName("plural_name")]
            public string? PluralName { get; set; }

            [JsonPropertyName("kind")]
            public string? Kind { get; set; } // person, place, or thing

            [JsonPropertyName("is_chronological")]
            public bool? IsChronological { get; set; }

            [JsonPropertyName("chronological_property")]
            public string? ChronologicalPropertyName { get; set; }
        }

        private class PropertyEnrichmentCompletion
        {
            [JsonPropertyName("properties")]
            public PropertyCompletion[]? Properties { get; set; }
        }

        private class GroupingEnrichmentCompletion
        {
            [JsonPropertyName("groupable_properties")]
            public string[][]? GroupablePropertyNames { get; set; }
        }

        private class PropertyCompletion
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("is_calculable")]
            public bool IsCalculable { get; set; }

            [JsonPropertyName("is_critical")]
            public bool IsCritical { get; set; }

            [JsonPropertyName("is_unique")]
            public bool IsUnique { get; set; }

            [JsonPropertyName("is_label")]
            public bool IsLabel { get; set; }

            [JsonPropertyName("is_mutable")]
            public bool IsMutable { get; set; }
        }

        private readonly IChatGptClient chatGptClient;
        private readonly ILogger log;
        private readonly IModelConfiguration modelConfig;

        public NounEnricher(
            IChatGptClient chatGptClient, 
            ILogger<NounEnricher> log,
            IModelConfiguration modelConfig)
        {
            this.chatGptClient = chatGptClient 
                ?? throw new ArgumentNullException(nameof(chatGptClient));

            this.log = log 
                ?? throw new ArgumentNullException(nameof(log));

            this.modelConfig = modelConfig
                ?? throw new ArgumentNullException(nameof(modelConfig));

            log.LogInformation($"Noun enrichment model: [{modelConfig.EnrichmentModelName}]");
        }

        private void ApplyGroupingEnrichmentCompletion(Noun noun, GroupingEnrichmentCompletion completion) =>
            noun.GroupableColumnSets = completion!.GroupablePropertyNames!;

        private void ApplyNounEnrichmentCompletion(Noun noun, NounEnrichmentCompletion completion)
        {
            noun.Kind = completion!.Kind;
            noun.SingularName = completion.SingularName;
            noun.PluralName = completion.PluralName;

            if (completion.IsChronological == true)
            {
                var chronProp = noun.Properties.Single(p => p.Alias == completion.ChronologicalPropertyName);

                if (chronProp.Type == ColumnTypes.DateTime)
                {
                    noun.IsChronological = true;
                    noun.ChronologicalSortPropertyName = completion.ChronologicalPropertyName;
                }
            }
        }

        private void ApplyPropertyEnrichmentCompletions(Noun noun, PropertyEnrichmentCompletion completion)
        {
            foreach (var property in noun.Properties)
            {
                var completionProperty = completion.Properties!
                    .FirstOrDefault(p => p.Name!.Equals(property.Alias, StringComparison.OrdinalIgnoreCase));

                if (completionProperty is not null)
                {
                    property.Title = completionProperty.Title;
                    property.Description = completionProperty.Description;
                    property.IsCritical = completionProperty.IsCritical;
                    property.IsUnique = completionProperty.IsUnique;
                    property.IsLabel = completionProperty.IsLabel;
                    property.IsMutable = completionProperty.IsMutable;

                    property.IsCalculable =
                        property.Type == ColumnTypes.Number &&
                        completionProperty.IsCalculable;
                }
            }
        }

        public async Task<Noun> Enrich(Noun noun, Understanding understanding)
        {
            ArgumentNullException.ThrowIfNull(noun, nameof(noun));
            ArgumentNullException.ThrowIfNull(understanding, nameof(understanding));

            try
            {
                if (noun.Properties.None()) return noun;

                log.LogInformation($"Enriching [{noun.Name}] understanding with semantic metadata...");

                var enrichPrompt = BuildNounEnrichmentPrompt(noun, understanding);

                await Complete<NounEnrichmentCompletion>(enrichPrompt)
                     .ContinueWith(t => ApplyNounEnrichmentCompletion(noun, t.Result!));

                var propGroups = noun.Properties
                    .GroupBy(p => p.SourceAlias ?? "_")
                    .ToDictionary(pg => pg.Key, pg => pg.ToList());

                var propEnrichTasks = new List<Task>();

                foreach (var sourceAlias in propGroups.Keys)
                {
                    log.LogInformation($"Enriching [{noun.Name}] [{sourceAlias}] properties with semantic metadata...");

                    var propertyPrompt = BuildPropertiesEnrichmentPrompt(
                        noun, propGroups[sourceAlias], understanding);

                    propEnrichTasks.Add(
                        Complete<PropertyEnrichmentCompletion>(propertyPrompt)
                        .ContinueWith(t => ApplyPropertyEnrichmentCompletions(noun, t.Result!)));
                }

                await Task.WhenAll(propEnrichTasks);

                var groupProps = noun.Properties.Where(
                    p => p.IsCritical && p.IsLabel && !p.IsUnique).ToList();

                if (groupProps.Any())
                {
                    var groupPrompt = BuildGroupablePropertiesEnrichmentPrompt(noun, groupProps, understanding);

                    await Complete<GroupingEnrichmentCompletion>(groupPrompt)
                         .ContinueWith(t => ApplyGroupingEnrichmentCompletion(noun, t.Result!));
                }

                return noun;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to enrich [{noun.Name}]. See inner exception for more details.", ex);
            }
        }

        private string BuildGroupablePropertiesEnrichmentPrompt(
            Noun noun, List<Property> candidateGroupProps, Understanding understanding) =>
           $$"""
             USE ONLY THE INFORMATION BELOW; MAKE NO ASSUMPTIONS.
             ADHERE STRICTLY TO THE PROMPT'S INSTRUCTIONS.

             SPEAK IN 3RD PERSON. GENERATE TEXTBOOK-QUALITY CONTENT.
             YOUR RESPONSE WILL INFORM LATER GPT PROMPTS SO EXPLAIN THINGS IN TERMS OF THE DOMAIN.

             ####
             
             Your role is to determine which PROPERTIES and/or PROPERTY SETS below (USE ONLY THE PROPERTIES BELOW) 
             would be ideal for GROUPING [{{noun.Name}}]s together to gain ACTIONABLE INTELLIGENCE.
             
             Ideal group properties are those that are HIGHLY MEANINGFUL to the CORE ANALYTICAL OBJECTIVES of the 
             domain in which the [{{noun.Name}}]s exist.

             USE ONLY THE FOLLOWING PROPERTIES:

             {{string.Join(", ", candidateGroupProps.Select(p => p.Alias!))}}

             ####

             INSTRUCTIONS:

             - GROUP HIERARCHICAL PROPERTIES TOGETHER IN THE SAME SET. THESE ARE ONLY EXAMPLES:
             -- e.g., [first_name, last_name] would be in the same set.
             -- e.g., [city, state, country] would be in the same set.
             -- e.g., [year, month, day] would be in the same set.
             -- e.g., [product_category, product_subcategory] would be in the same set.
             -- e.g., [building_number, room_number] would be in the same set.
             - GROUPS SHOULD REPRESENT HIGHER-ORDER SEGMENTS: Groups shouldn't be able to identify any one domain-specific person, place, or thing uniquely.
             -- They may, however, describe larger commonly known nouns such as locations, time, or well-known categories.

             1. IMAGINE you are creating a BI DASHBOARD for DOMAIN LEADERS to better UNDERSTAND THEIR {{noun.Name!.ToUpper()}}S.
             2. DETERMINE which CHARTS or VISUALS you would create to help DOMAIN LEADERS make STRATEGIC DECISIONS. 
             3. NOW, DETERMINE which PROPERTIES and/or PROPERTY SETS (FROM THE [PROPERTIES] SECTION BELOW) would be used to GROUP [{{noun.Name}}]s together to create those CHARTS.

             AGAIN, USE ONLY THE PROPERTIES LISTED ABOVE.

             ####

             RESPOND ONLY WITH A SINGLE JSON OBJECT FORMATTED EXACTLY LIKE THE ONE BELOW:
             
                    {
                        "groupable_properties": [
                            [
                                "<PROPERTY_NAME>",
                                "<PROPERTY_NAME>"
                            ],
                            [
                                "<PROPERTY_NAME>"
                            ], ...
                        ]
                    }

             ####

             PROPERTIES:

             The following describes all [{{noun.Name}}] properties:
             
             {{string.Join("\n", candidateGroupProps.Select(p => $"- [{p.Alias}]: {p.Description}"))}}

             ####

             The following describes the domain in which {{noun.Name}}s exist:
             
             {{understanding.Narrative!.DomainNarrative!.DomainDescription}}

             ####

             SOURCE SAMPLE SET:

             The following describes a random sample set of [{{noun.Name}}]s:
             
             {{understanding.NounSamples[noun.Name!]
               .SelectColumns(candidateGroupProps.Select(p => p.Alias!).ToArray())
               .Take(25).ToTsvTable()}}
             """;

        private string BuildPropertiesEnrichmentPrompt(
            Noun noun, List<Property> properties, Understanding understanding) =>
            $$"""
              RESPOND ONLY WITH A SINGLE JSON ARRAY MODELED EXACTLY LIKE THE ONE LATER IN THIS PROMPT; NO ADDITIONAL NARRATIVE.
              USE ONLY THE INFORMATION BELOW; MAKE NO ASSUMPTIONS.

              SPEAK IN 3RD PERSON. GENERATE TEXTBOOK-QUALITY CONTENT.
              YOUR RESPONSE WILL INFORM LATER GPT PROMPTS SO EXPLAIN THINGS IN TERMS OF THE DOMAIN.

              - <property_name> SHOULD ALWAYS BE THE ACTUAL NAME OF THE PROPERTY AS PROVIDED IN THE SAMPLE SET.
              - INCLUDE ALL {{properties.Count}} SAMPLE SET PROPERTIES [properties] IN YOUR RESPONSE.

              Your role is to enrich the {{noun.Name}} properties described below with additional semantic metadata.

              Answer the following questions about each {{noun.Name}} property based SOLELY on the information below:

              - Q1: How does EACH and EVERY property in the sample data above describe a [{{noun.Name}}]?
                - Describe EACH and EVERY property in a casual, clear, and conversational tone in two sentences or less.
              - Q2: What is the title of EACH and EVERY property as it is referred to in the real world?
                - Example: The property name is "first_name"; the real-world name is "first name". Always in lower case.
              - Q3: Which numeric properties in the sample data above would typically be used to perform calculations needed to gain actionable intelligence?
                 - DO NOT INCLUDE INCALCULABLE NUMERIC PROPERTIES LIKE IDs, SOCIAL SECURITY NUMBERs, POSTAL CODEs, PHONE NUMBERs, ETC.
              - Q4: Which properties in the sample data above are most likely to be unique to each [{{noun.Name}}]?
                 - Includes descriptions, unique identifiers, unique names, etc.
              - Q5: Which properties are labels and which are values?
                 - Labels are categorical and are used to organize [{{noun.Name}}]s into higher-order groups. 
                 - Values describe the specific [{{noun.Name}}] and are often distinct.
              - Q6: Which properties contain data ABSOLUTELY CRITICAL to the functioning of the larger domain?
                 - ASK YOURSELF THIS: if this data weren't available, would domain leaders have the all the actionable intelligence they need to make informed strategic domain decisions?
              - Q7: In the real world, which properties can be changed (are mutable)? Which are fixed and unchanging (immutable)?
                - Properties describe attributes of real world people, places, and things. In the real world, can this attribute be reasonably changed?
                - Mutable properties can change over time. Immutable properties are fixed and unchanging.
                - Example: a person's name is mutable; their date of birth is immutable.
                - Example: a product's price is mutable; its SKU is immutable.
                - Example: a location's name is mutable; its latitude and longitude are immutable.

              RESPOND ONLY WITH A SINGLE JSON ARRAY FORMATTED EXACTLY LIKE THE ONE BELOW:

              {
                "properties": [
                  {
                    "name": "<property_name>",
                    "description": "<description>",
                    "title": "<title>",
                    "is_calculable": <true or false>,
                    "is_critical": <true or false>,
                    "is_unique": <true or false>,
                    "is_label": <true or false>,
                    "is_mutable": <true or false>
                  }, ...
                ]
              }

              ####

              [{{noun.Name}}]s are part of the [{{understanding.ModelName}}] data model.
              
              The following describes the domain that the [{{understanding.ModelName}}] data model represents:
              
              {{understanding.Narrative!.DomainNarrative!.DomainDescription}}

              ####

              SAMPLE SET:

              HEADERS ARE PROPERTY NAMES.
              
              The following describes a random sample set of [{{noun.Name}}]s:
              
              {{understanding.NounSamples[noun.Name!]
                .SelectColumns(properties.Select(p => p.Alias!).ToArray())
                .Take(25).ToTsvTable()}}
              """;

        private string BuildNounEnrichmentPrompt(Noun noun, Understanding understanding) =>
           $$"""
             RESPOND ONLY WITH A SINGLE JSON OBJECT MODELED EXACTLY LIKE THE ONE AT THE BOTTOM OF THIS PROMPT; NO ADDITIONAL NARRATIVE.
             USE ONLY THE INFORMATION BELOW; MAKE NO ASSUMPTIONS.

             SPEAK IN 3RD PERSON. GENERATE TEXTBOOK-QUALITY CONTENT.
             YOUR RESPONSE WILL INFORM LATER GPT PROMPTS SO EXPLAIN THINGS IN TERMS OF THE DOMAIN.

             - DON'T USE PROVIDED PROPERTY AND NOUN NAMES DIRECTLY. INSTEAD, DESCRIBE THEM AS YOU WOULD IN THE REAL WORLD.
             - USE SHORT, EASILY-READ SENTENCES.
             - DESCRIBE IN A CASUAL AND FRIENDLY TONE. USE METAPHORS TO EXPLAIN COMPLEX CONCEPTS. THINK OF YOURSELF AS THE USER'S COWORKER AND FRIEND.
             - <property_name> SHOULD ALWAYS BE THE ACTUAL NAME OF THE PROPERTY AS PROVIDED.
             - INCLUDE ALL {{noun.Properties.Count}} PROPERTIES [properties] IN YOUR RESPONSE.

             Your role is to enrich the noun described below [{{noun.Name}}] with additional semantic metadata.
             
             Answer the following questions about [{{noun.Name}}]s based SOLELY on the information below:
             
             - Q1: In the real world, what is a {{noun.Name}} called in everyday language (singular_name)? Use lower case.
             - Q2: In the real world, what are a group of {{noun.Name}}s called in everyday language (plural_name)? Use lower case.
             - Q3: Are [{{noun.Name}}]s people, places, or things?
             - Q4: Are [{{noun.Name}}]s events ordered chronologically by a date/time property included in the sample data above?
                - If they are chronologically-ordered events, what is the name of the date/time property? Leave blank if not applicable.
             
             RESPOND ONLY WITH A SINGLE JSON OBJECT FORMATTED EXACTLY LIKE THE ONE BELOW:
             
                 {
                     "singular_name": "<singular_name>",
                     "plural_name": "<plural_name>",
                     "kind": "<people, places, or things>",
                     "is_chronological": <true or false>,
                     "chronological_property": "<property_name>"
                 }

             ####

             [{{noun.Name}}]s are part of the [{{understanding.ModelName}}] data model.
             
             The following describes the domain that the [{{understanding.ModelName}}] data model represents:

             {{understanding.Narrative!.DomainNarrative!.DomainDescription}}

             Here are some random [{{noun.Name}}]s from the [{{understanding.ModelName}}] data model:

             {{understanding.NounSamples[noun.Name!].Take(10).ToTsvTable()}}
             """;

        private async Task<T> Complete<T>(string prompt)
        {
            var retryPipeline = CreateRetryPipeline<T>();

            return await retryPipeline.ExecuteAsync(async token =>
            {
                var completion = await chatGptClient.Complete<T>(prompt, modelConfig.EnrichmentModelName);

                if (completion is null)
                {
                    throw new Exception($"Unable to complete prompt: [{nameof(chatGptClient)}] client returned null.");
                }

                return completion;
            });
        }
    }
}
