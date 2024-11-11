using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nounbase.Core.Extensions;
using Nounbase.Core.Interfaces.Enrichers;
using Nounbase.Core.Interfaces.Factories;
using Nounbase.Core.Interfaces.Narrators;
using Nounbase.Core.Interfaces.Samplers;
using Nounbase.Core.Models;
using Nounbase.Core.Models.Schema;
using Nounbase.Core.Models.Semantic;
using Nounbase.Core.Models.Semantic.Narrative;
using Nounbase.Core.Models.Semantic.Relational;
using static System.Environment;

namespace Nounbase.Console.GenMap
{
    public class MapFactory
    {
        private readonly ILogger log;

        private readonly INarrator narrator;
        private readonly INounEnricher enricher;
        private readonly INounFactory nounFactory;
        private readonly INounSampler nounSampler;
        private readonly ISchemaFactory schemaFactory;
        private readonly ITableSampler tableSampler;

        public MapFactory(
            ILogger<MapFactory> log,
            INarrator narrator,
            INounEnricher enricher,
            INounFactory nounFactory,
            INounSampler nounSampler,
            ISchemaFactory schemaFactory,
            ITableSampler tableSampler)
        {
            this.log = log;

            this.narrator = narrator;
            this.enricher = enricher;
            this.nounFactory = nounFactory;
            this.nounSampler = nounSampler;
            this.schemaFactory = schemaFactory;
            this.tableSampler = tableSampler;
        }

        public async Task<SemanticMap> CreateMap(MapConfiguration mapConfig)
        {
            try
            {
                log.LogInformation(
                    $"{NewLine}Semantically mapping schema [{mapConfig.SchemaName}]...{NewLine}");

                var understanding = new Understanding(mapConfig.SchemaName!);

                understanding.Schema = await ExtractSchema(mapConfig);
                understanding.TableSamples = await SampleTables(understanding);
                understanding.Nouns = await DiscoverNouns(understanding);
                understanding.NounSamples = await SampleNouns(understanding);
                understanding.Narrative = await Narrate(understanding);
                understanding.Nouns = await EnrichNouns(understanding);

                EnrichBranches(understanding);

                var map = CreateSemanticMap(understanding);

                return map;
            }
            catch
            {
                log.LogError(
                    $"{NewLine}Schema [{mapConfig.SchemaName}] semantic map creation failed. See exception details.{NewLine}");

                throw;
            }
        }

        private async Task SaveSemanticMap(SemanticMap map)
        {
            var outputPath =
                GetEnvironmentVariable(NounbaseEnv.MapOutputPath)
                ?? ".\\map.json";

            try
            {
                log.LogInformation(
                    $"Saving map to [{outputPath}]...");

                await File.WriteAllTextAsync(
                    outputPath,
                    JsonConvert.SerializeObject(map, Formatting.Indented));
            }
            catch
            {
                log.LogError($"Failed to save semantic map to [{outputPath}]. See exception details.");

                throw;
            }
        }

        private SemanticMap CreateSemanticMap(Understanding understanding)
        {
            try
            {
                log.LogInformation(
                    $"{NewLine}Creating semantic map...{NewLine}");

                var map = new SemanticMap(understanding.ModelName!);

                map.Domain = new Domain();
                map.Domain.Narrative = understanding.Narrative!.DomainNarrative;

                foreach (var noun in understanding.Nouns)
                {
                    noun.Narrative = understanding.Narrative!.NounNarratives[noun.Name!];
                    map.Domain.Nouns.Add(noun);
                }

                map.Schema = understanding.Schema;

                log.LogInformation(
                    $"{NewLine}Semantic map created.{NewLine}");

                return map;
            }
            catch
            {
                log.LogError(
                    $"Schema [{understanding.Schema!.Name}] semantic map creation failed. See exception details.");

                throw;
            }
        }

        private void EnrichBranches(Understanding understanding) =>
            understanding.Nouns = understanding.Nouns
                .Select(n => EnrichBranches(n, understanding))
                .ToList();

        private Noun EnrichBranches(Noun noun, Understanding understanding)
        {
            foreach (var _1stDegBranch in noun.Root!.Branches)
            {
                EnrichBranch(
                    _1stDegBranch,
                    understanding.Nouns.ByTableName(_1stDegBranch.TableName!)!);

                foreach (var _2ndDegBranch in _1stDegBranch.Branches)
                {
                    EnrichBranch(
                        _2ndDegBranch,
                        understanding.Nouns.ByTableName(_2ndDegBranch.TableName!)!);
                }
            }

            return noun;
        }

        private void EnrichBranch(Branch branch, Noun branchNoun)
        {
            var branchPropAliases = branch.Properties.Select(p => p.Alias).ToList();

            branch.Properties = branchNoun.Properties
                .Where(p => branchPropAliases.Contains(p.Alias))
                .ToList();

            if (branch.Properties.Any(p => p.Name == branchNoun.ChronologicalSortPropertyName))
            {
                branch.ChronologicalSortPropertyName = branchNoun.ChronologicalSortPropertyName;
                branch.IsChronological = branchNoun.IsChronological;
            }

            if (branchNoun.GroupableColumnSets != null)
            {
                branch.GroupableColumnSets = branchNoun.GroupableColumnSets
                    .Where(gcs => gcs.All(gc => branch.Properties.Any(p => p.Alias == gc)))
                    .ToArray();
            }
        }

        private async Task<IList<Noun>> EnrichNouns(Understanding understanding)
        {
            try
            {
                log.LogInformation(
                    $"{NewLine}Step 6: Enriching [{understanding.Schema!.Name}] nouns...{NewLine}");

                var enrichTasks = new List<Task<Noun>>();

                foreach (var noun in understanding.Nouns)
                {
                    enrichTasks.Add(EnrichNoun(noun, understanding));
                }

                await Task.WhenAll(enrichTasks);

                log.LogInformation(
                    $"{NewLine}[{enrichTasks.Count}] nouns enriched.{NewLine}");

                return enrichTasks.Select(t => t.Result).ToList();
            }
            catch
            {
                log.LogError(
                    $"Schema [{understanding.Schema!.Name}] noun enrichment failed. See exception details.");

                throw;
            }
        }

        private async Task<Noun> EnrichNoun(Noun noun, Understanding understanding)
        {
            try
            {
                log.LogInformation(
                    $"Enriching noun [{noun.Name}]...");

                var enrichedNoun = await enricher.Enrich(noun, understanding);

                log.LogInformation(
                    $"Noun [{noun.Name}] enriched.");

                return enrichedNoun;
            }
            catch
            {
                log.LogError(
                    $"Noun [{noun.Name}] enrichment failed. See exception details.");

                throw;
            }
        }

        private async Task<ModelNarrative> Narrate(Understanding understanding)
        {
            try
            {
                log.LogInformation(
                    $"{NewLine}Step 5: Telling [{understanding.Schema!.Name}]'s story...{NewLine}");

                var narrative = await narrator.Narrate(understanding);

                log.LogInformation(
                    $"{NewLine}Ooo what a compelling story of " +
                    $"[{understanding.Nouns.Count(n => n.Kind == "person")}] people, " +
                    $"[{understanding.Nouns.Count(n => n.Kind == "place")}] places, and " +
                    $"[{understanding.Nouns.Count(n => n.Kind == "thing")}] things!{NewLine}");

                return narrative;
            }
            catch
            {
                log.LogError(
                    $"Schema [{understanding.Schema!.Name}] narration failed. See exception details.");

                throw;
            }   
        }

        private async Task<IDictionary<string, DbRecordSet>> SampleNouns(Understanding understanding)
        {
            try
            {
                log.LogInformation(
                    $"{NewLine}Step 4: Sampling [{understanding.Schema!.Name}] nouns...{NewLine}");

                var sampleTasks = new Dictionary<string, Task<DbRecordSet?>>();

                foreach (var noun in understanding.Nouns)
                {
                    sampleTasks.Add(
                        noun.Name!, 
                        SampleNoun(noun, understanding));
                }

                await Task.WhenAll(sampleTasks.Values);

                var samples =
                    sampleTasks
                   .Select(t => new { NounName = t.Key, Sample = t.Value.Result })
                   .Where(t => t.Sample != null)
                   .ToDictionary(t => t.NounName, t => t.Sample!);

                if (samples.Any())
                {
                    log.LogInformation(
                        $"{NewLine}[{samples.Count}] sample sets taken.");

                    log.LogInformation(
                        $"[{samples.Sum(s => s.Value.RowCount)}] total samples taken.{NewLine}");
                }
                else
                {
                    log.LogWarning(
                        "No samples taken. Nounbase has no idea what this database contains."); // It really doesn't.
                }

                return samples;
            }
            catch
            {
                log.LogError(
                    $"Schema [{understanding.Schema!.Name}] noun sampling failed. See exception details.");

                throw;
            }
        }

        private async Task<DbRecordSet?> SampleNoun(Noun noun, Understanding understanding)
        {
            try
            {
                log.LogInformation(
                    $"Sampling noun [{noun.Name}]...");

                var sampleSet = await
                    nounSampler.GetNounSampleSet(noun, understanding);

                if (sampleSet == null)
                {
                    log.LogInformation(
                        $"[{noun.Name}] has no 'human' properties. No samples taken.");
                }
                else
                {
                    log.LogInformation(
                        $"[{sampleSet.RowCount}] row(s) sampled from noun [{noun.Name}].");
                }

                return sampleSet;
            }
            catch
            {
                log.LogError(
                    $"Noun [{noun.Name}] sampling failed. See exception details.");

                throw;
            }
        }

        private async Task<IList<Noun>> DiscoverNouns(Understanding understanding)
        {
            try
            {
                log.LogInformation($"{NewLine}Step 3: Discovering [{understanding.Schema!.Name}] nouns...{NewLine}");

                var discoverTasks = new Dictionary<string, Task<Noun>>();
                var semaphore = new SemaphoreSlim(0, 10);

                foreach (var table in understanding.Schema!.Tables)
                {
                    discoverTasks.Add(table.Name!, DiscoverNoun(table, understanding));
                }

                await Task.WhenAll(discoverTasks.Values);

                var nouns = discoverTasks.Select(t => t.Value.Result).ToList();

                ExpandNouns(nouns, understanding);
                ExpandBranches(nouns);

                return nouns;
            }
            catch
            {
                log.LogError($"Schema [{understanding.Schema!.Name}] noun discovery failed. See exception details.");

                throw;
            }
        }

        private void ExpandNouns(IList<Noun> nouns, Understanding understanding)
        {
            var dimProperties = new Dictionary<string, List<Property>>();

            foreach (var noun in nouns)
            {
                foreach (var dimension in noun.Dimensions)
                {
                    dimension.MermaidDefinition =
                        $"\"{dimension.NounName}\" ||--o{{ \"{noun.Name}\" : \"{dimension.Alias}\"";

                    dimension.Dimensions.AddRange(
                        GetSubdimensions(understanding.Schema!, dimension, nouns!));
                }
            }

            foreach (var noun in nouns)
            {
                dimProperties.Add(noun.Name!, noun.Properties.ToList());

                foreach (var dimension in noun.Dimensions)
                {
                    dimProperties[noun.Name!].AddRange(
                        GetDimensionalProperties(dimension, nouns!));
                }
            }

            foreach (var noun in nouns)
            {
                noun.Properties = dimProperties[noun.Name!];
            }
        }

        private void ExpandBranches(IList<Noun> nouns)
        {
            foreach (var noun in nouns)
            {
                foreach (var _1stDegBranch in noun.Root!.Branches)
                {
                    var _1stDegNoun = nouns.ByTableName(_1stDegBranch.TableName!)!;

                    var _1stDegDim = _1stDegNoun.Dimensions.Single(
                        d => d.ForeignKeyName == _1stDegBranch.ForeignKey!.Name);

                    _1stDegBranch.Dimensions.AddRange(_1stDegNoun.Dimensions
                        .Where(d => d.Lineage.None() || d.Lineage[0] != _1stDegDim.Alias));

                    _1stDegBranch.Properties.AddRange(_1stDegNoun.Properties
                        .Where(p => p.Lineage.None() || p.Lineage[0] != _1stDegDim.Alias));

                    foreach (var _2ndDegBranch in _1stDegBranch.Branches)
                    {
                        var _2ndDegNoun = nouns.ByTableName(_2ndDegBranch.TableName!)!;

                        var _2ndDegDim = _2ndDegNoun.Dimensions.Single(
                            d => d.ForeignKeyName == _2ndDegBranch.ForeignKey!.Name);

                        _2ndDegBranch.Dimensions.AddRange(_2ndDegNoun.Dimensions
                            .Where(d => d.Lineage.None() || d.Lineage[0] != _2ndDegDim.Alias));

                        _2ndDegBranch.Properties.AddRange(_2ndDegNoun.Properties
                            .Where(p => p.Lineage.None() || p.Lineage[0] != _2ndDegDim.Alias));
                    }
                }
            }
        }

        private IEnumerable<Dimension> GetSubdimensions(Schema schema, Dimension dimension, IList<Noun> nouns)
        {
            var subDimensions = new List<Dimension>();
            var dimensionFk = schema.ForeignKeys.Single(fk => fk.Name == dimension.ForeignKeyName);
            var dimensionNoun = nouns.Single(n => n.Name == dimensionFk.PrimaryKeyRef!.TableName);

            foreach (var childDimension in dimensionNoun.Dimensions.Where(d => d.ForeignKeyName != dimension.ForeignKeyName))
            {
                var subDimension = new Dimension($"{dimension.Alias}_{childDimension.Alias}",
                    childDimension.ForeignKeyName!, childDimension.NounName!);

                var alias = new string(subDimension.Alias!.Skip(dimension.Alias!.Count() + 1).ToArray());

                subDimension.MermaidDefinition =
                    $"\"{subDimension.NounName}\" ||--o{{ \"{dimension.NounName}\" : \"{alias}\"";

                subDimension.Lineage.AddRange(dimension.Lineage); // Inherits parent lineage
                subDimension.Lineage.Add(subDimension.Alias!);    // + its own leaf

                subDimension.Dimensions.AddRange(GetSubdimensions(schema, subDimension, nouns));

                subDimensions.Add(subDimension);
            }

            return subDimensions;
        }

        private IEnumerable<Property> GetDimensionalProperties(Dimension dimension, IList<Noun> nouns)
        {
            var properties = new List<Property>();
            var dimNoun = nouns.Single(n => n.Name == dimension.NounName);

            properties.AddRange(dimNoun.Properties
                .Select(p => new Property
                {
                    Name = p.Name,
                    Type = p.Type,
                    Description = p.Description,
                    Alias = $"{dimension.Alias}_{p.Name}",
                    ForeignKeyName = dimension.ForeignKeyName,
                    SourceAlias = dimension.Alias,
                    SourceTable = dimNoun.TableName,
                    Lineage = dimension.Lineage // Properties inherit their dimension's lineage
                }));

            foreach (var childDimension in dimension.Dimensions.Where(d => d.Alias != dimension.Alias))
            {
                properties.AddRange(GetDimensionalProperties(childDimension, nouns));
            }

            return properties;
        }

        private async Task<Noun> DiscoverNoun(Table table, Understanding understanding)
        {
            try
            {
                log.LogInformation($"Discovering noun [{table.Name}]...");

                var noun = await nounFactory.CreateNoun(table, understanding);

                log.LogInformation($"Noun [{table.Name}] discovered.");

                return noun;
            }
            catch
            {
                log.LogError($"{NewLine}Noun [{table.Name}] discovery failed. See exception details.{NewLine}");

                throw;
            }
        }

        private async Task<Schema> ExtractSchema(MapConfiguration mapConfig)
        {
            try
            {
                log.LogInformation(
                    $"{NewLine}Step 1: Extracting schema [{mapConfig.SchemaName}]...{NewLine}");

                var schema = await schemaFactory.CreateSchema(mapConfig.SchemaName!);

                log.LogInformation(
                    $"Schema [{mapConfig.SchemaName}] extracted. " +
                    $"It contains [{schema.Tables.Count}] table(s) and [{schema.ForeignKeys.Count}] foreign key(s).");

                return schema;
            }
            catch
            {
                log.LogError(
                    $"Schema [{mapConfig.SchemaName}] extraction failed. See exception details.");

                throw;
            }
        }

        private async Task<IDictionary<string, DbRecordSet>> SampleTables(Understanding understanding)
        {
            try
            {
                log.LogInformation($"{NewLine}Step 2: Sampling tables in schema [{understanding.Schema!.Name}]...{NewLine}");

                var sampleTasks = new Dictionary<string, Task<DbRecordSet>>();

                foreach (var table in understanding.Schema!.Tables)
                {
                    sampleTasks.Add(table.Name!, SampleTable(table, understanding));
                }

                await Task.WhenAll(sampleTasks.Values);

                return sampleTasks.ToDictionary(t => t.Key, t => t.Value.Result);
            }
            catch
            {
                log.LogError($"Schema [{understanding.Schema!.Name}] table sampling failed. See exception details.");

                throw;
            }
        }

        private async Task<DbRecordSet> SampleTable(Table table, Understanding understanding)
        {
            try
            {
                log.LogInformation($"Sampling table [{table.Name}]...");

                var sampleSet = await tableSampler.GetTableSampleSet(table, understanding);

                log.LogInformation($"[{sampleSet.RowCount}] row(s) sampled from table [{table.Name}].");

                return sampleSet;
            }
            catch
            {
                log.LogError($"Table [{table.Name}] sampling failed. See exception details.");

                throw;
            }
        }
    }
}