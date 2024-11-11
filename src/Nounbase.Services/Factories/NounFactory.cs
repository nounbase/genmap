using Microsoft.Extensions.Logging;
using Nounbase.Core.Extensions;
using Nounbase.Core.Interfaces.Clients;
using Nounbase.Core.Interfaces.Configuration;
using Nounbase.Core.Interfaces.Factories;
using Nounbase.Core.Models;
using Nounbase.Core.Models.Schema;
using Nounbase.Core.Models.Semantic;
using Nounbase.Core.Models.Semantic.Relational;
using System.Text;
using System.Text.Json;

namespace Nounbase.Services.Factories
{
    public class NounFactory : INounFactory
    { 
        private readonly IChatGptClient chatGptClient;
        private readonly ILogger log;
        private readonly IModelConfiguration modelConfig;

        public NounFactory(IChatGptClient chatGptClient, ILogger<NounFactory> log, IModelConfiguration modelConfig)
        {
            ArgumentNullException.ThrowIfNull(chatGptClient, nameof(chatGptClient));
            ArgumentNullException.ThrowIfNull(log, nameof(log));
            ArgumentNullException.ThrowIfNull(modelConfig, nameof(modelConfig));

            this.chatGptClient = chatGptClient;
            this.log = log;
            this.modelConfig = modelConfig;

            log.LogInformation($"Noun discovery model: [{modelConfig.DiscoveryModelName}].");
        }

        public async Task<Noun> CreateNoun(Table table, Understanding understanding)
        {
            ArgumentNullException.ThrowIfNull(table, nameof(table));
            ArgumentNullException.ThrowIfNull(understanding, nameof(understanding));

            try
            {
                var noun = new Noun(table, understanding.Schema!.Name!);

                noun.Dimensions = await GetDimensions(table, understanding);
                noun.Properties = await GetConversationalProperties(table, understanding);
                noun.Root = CreateGraph(table, understanding);

                return noun;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Noun [{table.Name}] creation failed. See inner exception for more details.", ex);
            }
        }

        private string BuildConversationalPropertiesPrompt(Table table, Understanding understanding) =>
            $$"""
              RETURN ONLY A JSON ARRAY OF PROPERTY NAMES.
              DO NOT INCLUDE ID, APP-SPECIFIC, OR ANY COLUMN WHICH DOESN'T MEANINGFULLY DESCRIBE THE SAMPLE DATA.
              DO INCLUDE COLUMNS THAT WOULD BE GOOD FOR ANALYSIS AND SEGMENTATION.

              Based on the TSV sample from the [{{table.Name}}] table below and table/column names, which columns contain
              information that ALONE OR THROUGH ANALYSIS would likely be included in everyday verbal conversation about the real-world 
              people, places, and things that the sample data describes?

              - DON'T INCLUDE ANY UTILITY COLUMNS: app/database-specific flags, tracking fields, etc.

              Sample data:

              {{understanding.TableSamples[table.Name!].ToTsvTable()}}
              """;

        private string BuildDimensionsPrompt(Table table, Understanding understanding)
        {
            var dimensionFks = understanding.Schema!.ForeignKeys
                .Where(fk => fk.ForeignKeyRef!.TableName == table.Name)
                .ToList();

            var builder = new StringBuilder(
                $"The information below describes the [{table.Name}] table and its related dimension tables. " +
                "Based only in the below, create descriptive, meaningful, and short SQL join aliases for each dimension table formatted \"like_this\". " +
                "Alias names can not be existing column names");

            builder.AppendLine();
            builder.AppendLine("- RETURN ONLY A JSON OBJECT.");
            builder.AppendLine("- EACH PROPERTY SHOULD BE THE NAME OF A FOREIGN KEY COLUMN.");
            builder.AppendLine("- EACH PROPERTY VALUE SHOULD BE THAT FOREIGN KEY RELATIONSHIP'S ALIAS.");
            builder.AppendLine("- ALIAS SHOULD DESCRIBE REAL-WORLD RELATIONSHIP.");
            builder.AppendLine("- ALIAS SHOULD NOT INCLUDE DATABASE TERMS LIKE TABLE OR RELATIONSHIP.");
            builder.AppendLine("- NO ADDITIONAL NARRATIVE.");
            builder.AppendLine("- THE JSON OBJECT SHOULD CONTAIN THESE PROPERTIES: ");
            builder.AppendLine();

            builder.AppendLine(JsonSerializer.Serialize(dimensionFks.ToDictionary(
                fk => fk.ForeignKeyRef!.ColumnName!,
                fk => "table alias")));

            builder.AppendLine();

            foreach (var dimensionFk in dimensionFks)
            {
                var fkTableName = $"[{table.Name}]";
                var pkTableName = $"[{dimensionFk.PrimaryKeyRef!.TableName}]";

                var fkColumnName = $"{fkTableName}.[{dimensionFk.ForeignKeyRef!.ColumnName}]";
                var pkColumnName = $"{pkTableName}.[{dimensionFk.PrimaryKeyRef!.ColumnName}]";

                builder.AppendLine(
                    $"- There is a one-to-many relationship between the {pkTableName} and {fkTableName} tables " +
                    $"joined through the {pkColumnName} primary key and {fkColumnName} foreign key columns.");
            }

            builder.AppendLine($"Root table [{table.Name}] samples: ");
            builder.AppendLine();
            builder.AppendLine(understanding.TableSamples[table.Name!].ToTsvTable());
            builder.AppendLine();

            foreach (var dimensionFk in dimensionFks)
            {
                var pkTableName = dimensionFk.PrimaryKeyRef!.TableName!;

                builder.AppendLine($"Dimension table [{pkTableName}] samples: ");
                builder.AppendLine();
                builder.AppendLine(understanding.TableSamples![pkTableName].ToTsvTable());
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private async Task<List<Property>> GetConversationalProperties(Table table, Understanding understanding)
        {
            var prompt = BuildConversationalPropertiesPrompt(table, understanding);

            var completion = 
                (await chatGptClient.Complete(prompt, modelConfig.DiscoveryModelName))
                .RemoveJsonLabels()!;

            if (string.IsNullOrEmpty(completion))
            {
                throw new InvalidOperationException(
                    $"No [{modelConfig.DiscoveryModelName}] [{table.Name}] [{nameof(GetConversationalProperties)}] " +
                     "completion generated.");
            }

            var humanPropertyNames = JsonSerializer.Deserialize<IEnumerable<string>>(completion);

            if (humanPropertyNames == null)
            {
                throw new InvalidOperationException(
                    $"Unexpected [{modelConfig.DiscoveryModelName}] [{table.Name}] [{nameof(GetConversationalProperties)}] " +
                    $"completion generated: [\"{completion}\"].");
            }

            var properties = new List<Property>();

            foreach (var column in table.Columns // Don't include any key columns in the list of human properties.
                                        .Where(p => !understanding.Schema!.ForeignKeys
                                        .Any(fk => (fk.ForeignKeyRef!.TableName == table.Name &&
                                                    fk.ForeignKeyRef!.ColumnName == p.Name) ||
                                                    table.PrimaryKeys.Contains(p.Name!))))
            {
                if (humanPropertyNames.Contains(column.Name))
                {
                    properties.Add(new Property
                    {
                        Alias = column.Name,
                        Name = column.Name,
                        SourceTable = table.Name,
                        Type = column.Type
                    });
                }
            }

            return properties;
        }

        private async Task<List<Dimension>> GetDimensions(Table table, Understanding understanding)
        {
            try
            {
                var dimensions = new List<Dimension>();

                if (understanding.Schema!.ForeignKeys.Any(
                    fk => fk.ForeignKeyRef!.TableName == table.Name))
                {
                    var prompt = BuildDimensionsPrompt(table, understanding);

                    var completion = 
                        (await chatGptClient.Complete(prompt, modelConfig.DiscoveryModelName))
                        .RemoveJsonLabels()!;

                    if (string.IsNullOrEmpty(completion))
                    {
                        throw new InvalidOperationException(
                            $"No [{modelConfig.DiscoveryModelName}] [{table.Name}] [{nameof(GetDimensions)}] " +
                             "completion generated.");
                    }

                    var aliases = JsonSerializer.Deserialize<Dictionary<string, string>>(completion);

                    if (aliases == null)
                    {
                        throw new InvalidOperationException(
                            $"Unexpected [{modelConfig.DiscoveryModelName}] [{table.Name}] [{nameof(GetDimensions)}] " +
                            $"completion generated: [\"{completion}\"].");
                    }

                    foreach (var fkColumnName in aliases.Keys)
                    {
                        var dimensionFk = understanding.Schema!.ForeignKeys.Single(
                            fk => fk.ForeignKeyRef!.ColumnName == fkColumnName && fk.ForeignKeyRef!.TableName == table.Name);

                        var alias = aliases[fkColumnName];

                        dimensions.Add(new Dimension
                        {
                            Alias = alias,
                            ForeignKeyName = dimensionFk.Name,
                            NounName = dimensionFk.PrimaryKeyRef!.TableName,
                            Lineage = [ alias ]
                        });
                    }
                }

                return dimensions;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get [{table.Name}] dimensions. See inner exception for more details.", ex);
            }
        }

        private Root CreateGraph(Table table, Understanding understanding)
        {
            var tree = new Root();

            foreach (var _1stDegreeFk in understanding.Schema!.ForeignKeys.Where(
                     fk => fk.PrimaryKeyRef!.TableName == table.Name))
            {
                var _1stDegreeBranch = new Branch(_1stDegreeFk.ForeignKeyRef!.TableName!, _1stDegreeFk);

                foreach (var _2ndDegreeFk in understanding.Schema!.ForeignKeys.Where(
                         fk => fk.PrimaryKeyRef!.TableName == _1stDegreeBranch.TableName!))
                {
                    _1stDegreeBranch.Branches.Add(new Branch(_2ndDegreeFk.ForeignKeyRef!.TableName!, _2ndDegreeFk));
                }

                tree.Branches.Add(_1stDegreeBranch);
            }

            return tree;
        }
    }
}
