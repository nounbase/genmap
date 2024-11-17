using Microsoft.Extensions.Logging;
using Nounbase.Core.Extensions;
using Nounbase.Core.Interfaces.Clients;
using Nounbase.Core.Interfaces.Configuration;
using Nounbase.Core.Interfaces.Narrators;
using Nounbase.Core.Models;
using Nounbase.Core.Models.Schema;
using Nounbase.Core.Models.Semantic;
using Nounbase.Core.Models.Semantic.Narrative;
using System.Text;

namespace Nounbase.Services.Narrators
{
    public class Narrator : INarrator
    {
        private readonly IChatGptClient chatGptClient;
        private readonly ILogger log;
        private readonly IModelConfiguration modelConfig;

        public Narrator(IChatGptClient chatGptClient, ILogger<Narrator> log, IModelConfiguration modelConfig)
        {
            ArgumentNullException.ThrowIfNull(chatGptClient, nameof(chatGptClient));
            ArgumentNullException.ThrowIfNull(log, nameof(log));
            ArgumentNullException.ThrowIfNull(modelConfig, nameof(modelConfig));

            this.chatGptClient = chatGptClient;
            this.log = log;
            this.modelConfig = modelConfig;

            log.LogInformation($"Narration model: [{modelConfig.NarrationModelName}]");
        }

        public async Task<ModelNarrative> Narrate(Understanding understanding)
        {
            ArgumentNullException.ThrowIfNull(understanding, nameof(understanding));

            try
            {
                var mapNarrative = new ModelNarrative();

                log.LogInformation("Writing model domain narrative...");

                var domainNarrationPrompt = BuildDomainNarrationPrompt(understanding);
                var domainNarration = await chatGptClient.Complete(domainNarrationPrompt, modelConfig.NarrationModelName);

                mapNarrative.DomainNarrative = new DomainNarrative(domainNarration!);

                log.LogInformation("Model domain narrative complete.");

                var getNounNarrations = new Dictionary<string, Task<NounNarrative>>();

                foreach (var noun in understanding.Nouns)
                {
                    log.LogInformation($"Writing [{noun.Name}] domain narrative...");

                    var nounNarrationPrompt = BuildNounNarrationPrompt(
                        noun, mapNarrative.DomainNarrative, understanding);

                    getNounNarrations.Add(
                        noun.Name!, 
                        chatGptClient.Complete<NounNarrative>(
                            nounNarrationPrompt, modelConfig.NarrationModelName)!);
                }

                await Task.WhenAll(getNounNarrations.Values);

                foreach (var nounName in getNounNarrations.Keys)
                {
                    log.LogInformation($"[{nounName}] domain narrative complete.");

                    mapNarrative.NounNarratives.Add(nounName, getNounNarrations[nounName].Result);
                }

                return mapNarrative;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to narrate model [{understanding.ModelName}]. See inner exception for more details.", ex); 
            }
        }

        private string BuildDomainNarrationPrompt(Understanding understanding)
        {
            var builder = new StringBuilder(
                """
                SPEAK IN 3RD PERSON. GENERATE TEXTBOOK-QUALITY CONTENT.
                YOUR RESPONSE WILL INFORM LATER GPT PROMPTS SO EXPLAIN THINGS IN TERMS OF THE DOMAIN.

                ROLE:

                You role is to describe the REAL-WORLD DOMAIN represented by the following 
                MODEL and SAMPLE DATA while ALWAYS ADHERING to the provided INSTRUCTIONS.

                ####

                INSTRUCTIONS:

                - RETURN ONLY THE ANSWER: Provide no other content.
                - USE ONLY THE INFORMATION BELOW: Make no assumptions. Inaccurate information is dangerous.
                - LOOK BEYOND THE MODEL: Read the SAMPLE DATA and use it to describe the domain.
                - IMAGINE YOU'RE A PERSON DESCRIBED BY THE MODEL: What would you say? How would you describe things?
                - AMSWER THIS QUESTION: What PEOPLE, PLACES, and THINGS make up this domain? How do they INTERACT?
                - DESRIBE THE DOMAIN'S OPERATIONS: How do these PEOPLE, PLACES, and THINGS interact?
                - DESCRIBE THE DOMAIN AS YOU WOULD IN THE REAL WORLD: Use short, easily-read sentences.
                - PARAGRAPHS SHOULD ALWAYS BE SURROUNDED BY [p] AND [/p] TAGS: Use no other formatting.
                - DESCRIBE IN 150 WORDS OR LESS: Use no more than 5 sentences per paragraph.

                Describe as COMPLETELY as possible in a CASUAL, CLEAR, and CONVERSATIONAL tone 
                the domain represented by the following MODEL and SAMPLE DATA.
                
                - The general purpose and domain that this MODEL is designed to represent or manage.
                - The key PEOPLE, PLACES, and THINGS and their INTERACTIONS represented by this MODEL.
                - Any logical RULES or PATTERNS that can be inferred from the MODEL.
                - OBSERVATIONS or INSIGHTS about the domain, considering the MODEL and SAMPLE DATA provided.

                ###3

                MODEL:
                """);

            foreach (var noun in understanding.Nouns)
            {
                builder.AppendLine();
                builder.AppendLine($"- The model contains [{noun.Name}]s.");

                foreach (var branch in noun.Root!.Branches)
                {
                    var branchNoun = understanding.Nouns.ByTableName(branch.TableName!)!;

                    builder.AppendLine($"- Each [{noun.Name}]s can have one or more [{branchNoun.Name}]s.");
                }

                builder.AppendLine("SAMPLE SOURCE DATA:");
                builder.AppendLine();

                if (understanding.NounSamples.ContainsKey(noun.Name!))
                {
                    builder.AppendLine(BuildSamplesPrompt(noun, understanding.NounSamples[noun.Name!], 10));
                }
            }

            return builder.ToString();
        }

        private string BuildNounNarrationPrompt(
            Noun noun, DomainNarrative domainNarrative, Understanding understanding)
        {
            var builder = new StringBuilder(
                $$"""
                  SPEAK IN 3RD PERSON. GENERATE TEXTBOOK-QUALITY CONTENT.
                  YOUR RESPONSE WILL INFORM LATER GPT PROMPTS SO EXPLAIN THINGS IN TERMS OF THE DOMAIN.

                  RETURN ONLY A VALID JSON OBJECT AND NO ADDITIONAL CONTENT.

                  ROLE:

                  YOU'RE PART OF THE DOMAIN DESCRIBED BY THE MODEL AND SAMPLE SOURCE DATA BELOW.
                  You're role is to describe {{noun.Name!.ToUpper()}}S as described in the NOUN DESC, 
                  MODEL DESC, and SAMPLE DATA while ALWAYS ADHERING to the provided INSTRUCTIONS.
                  
                  ####
                  
                  INSTRUCTIONS:

                  ONLY RESPOND IN THE JSON FORMAT PROVIDED BELOW.
                  
                  - RETURN ONLY THE ANSWER: Provide no other content.
                  - USE ONLY THE INFORMATION BELOW: Make no assumptions. Inaccurate information is dangerous.
                  - LOOK BEYOND THE NOUN AND MODEL DESC: Read the SAMPLE DATA and use it to describe the domain.
                  - IMAGINE YOU'RE A PERSON DESCRIBED BY THE MODEL: What would you say? How would you describe things?
                  - DESCRIBE THE DOMAIN AS YOU WOULD IN THE REAL WORLD: Use short, easily-read sentences.
                  - PARAGRAPHS SHOULD ALWAYS BE SURROUNDED BY [p] AND [/p] TAGS: Use no other formatting.
                  - DESCRIBE IN 150 WORDS OR LESS: Use no more than 5 sentences per paragraph.

                  ASK YOURSELF THIS:

                  - WHAT IS A [{{noun.Name!}}]?
                  -- HOW does it INTERACT with other PEOPLE in the domain?
                  -- HOW does it INTERACT with other PLACES in the domain?
                  -- HOW does it INTERACT with other THINGS in the domain?

                  ####

                  JSON FORMAT:

                  {
                    "what_is": "<what is a {{noun.Name!}}?>",
                    "interact_with_other": {
                        "people": "<how does it interact with other PEOPLE in the domain?>",
                        "places": "<how does it interact with other PLACES in the domain?>",
                        "things": "<how does it interact with other THINGS in the domain?>"
                    }
                  }

                  ####

                  MODEL DESC:

                  {{domainNarrative.DomainDescription}}

                  ####

                  NOUN DESC:
                  """);

            if (noun.Properties.Any() && 
                understanding.NounSamples.ContainsKey(noun.Name!))
            {
                builder.AppendLine(BuildSamplesPrompt(noun, understanding.NounSamples[noun.Name!], 10));
            }

            foreach (var _1stDegBranch in noun.Root!.Branches)
            {
                var _1stDegNoun = understanding.Nouns.ByTableName(_1stDegBranch.TableName!)!;

                builder.AppendLine($"- Each [{noun.Name}] can have one or more [{_1stDegNoun.Name}]s.");

                if (_1stDegNoun.Properties.Any() && 
                    understanding.NounSamples.ContainsKey(_1stDegNoun.Name!))
                {
                    builder.AppendLine("SAMPLE DATA:");
                    builder.AppendLine();

                    builder.AppendLine(BuildSamplesPrompt(_1stDegNoun, understanding.NounSamples[_1stDegNoun.Name!], 10));
                }

                foreach (var _2ndDegBranch in _1stDegBranch.Branches)
                {
                    var _2ndDegNoun = understanding.Nouns.ByTableName(_2ndDegBranch.TableName!)!;

                    builder.AppendLine($"- Each [{_1stDegNoun.Name}] can have one or more [{_2ndDegNoun.Name}]s.");

                    if (_2ndDegNoun.Properties.Any() &&
                        understanding.NounSamples.ContainsKey(_2ndDegNoun.Name!))
                    {
                        builder.AppendLine("SAMPLE DATA:");
                        builder.AppendLine();

                        builder.AppendLine(BuildSamplesPrompt(_2ndDegNoun, understanding.NounSamples[_2ndDegNoun.Name!], 10));
                    }
                }
            }

            return builder.ToString().RemoveJsonLabels()!;
        }

        private string BuildSamplesPrompt(Noun noun, DbRecordSet sample, int take = 5) =>
            $"- Here are some sample [{noun.Name}]s:\n\n{sample.Take(take).ToTsvTable()}\n";
    }
}
