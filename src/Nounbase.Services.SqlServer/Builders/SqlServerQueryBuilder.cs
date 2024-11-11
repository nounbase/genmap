using Nounbase.Core.Interfaces.Builders;
using Nounbase.Core.Models;
using Nounbase.Core.Models.Semantic;

namespace Nounbase.Services.SqlServer.Builders
{
    public class SqlServerQueryBuilder : BaseSqlServerQueryBuilder, IQueryBuilder
    {
        public string BuildSampleQuery(Noun noun, Understanding understanding)
        {
            const int sampleSize = 25;

            ArgumentNullException.ThrowIfNull(noun, nameof(noun));
            ArgumentNullException.ThrowIfNull(understanding, nameof(understanding));

            return $$"""
                     SELECT     TOP {{sampleSize}} {{string.Join("\n          ,", GetSelectableColumns(noun))}}
                     FROM       [{{understanding.Schema!.Name}}].[{{noun.TableName}}] AS [{{noun.TableName}}]
                     {{string.Join('\n', GetDimensionalJoins(noun, understanding))}}
                     ORDER BY   newid()
                     """;
        }

        protected IEnumerable<string> GetDimensionalJoins(Noun noun, Understanding understanding)
        {
            var joins = new List<string>();

            foreach (var dimension in noun.Dimensions)
            {
                var dimNoun = understanding.Nouns!.Single(n => n.Name == dimension.NounName);
                var dimKey = understanding.Schema!.ForeignKeys!.Single(fk => fk.Name == dimension.ForeignKeyName);

                joins.Add(
                    $$"""
                      LEFT JOIN  [{{understanding.Schema!.Name}}].[{{dimNoun.TableName}}] AS [{{dimension.Alias}}]
                      ON         [{{noun.TableName}}].[{{dimKey.ForeignKeyRef!.ColumnName}}] = 
                                 [{{dimension.Alias}}].[{{dimKey.PrimaryKeyRef!.ColumnName}}]
                      """);

                joins.AddRange(GetDimensionalJoins(dimension, understanding));
            }

            return joins;
        }

        protected IEnumerable<string> GetDimensionalJoins(Dimension dimension, Understanding understanding)
        {
            var joins = new List<string>();

            foreach (var subDimension in dimension.Dimensions.Where(d => d.Alias != dimension.Alias)) // No circular references
            {
                var subDimNoun = understanding.Nouns!.Single(n => n.Name == subDimension.NounName);
                var subDimKey = understanding.Schema!.ForeignKeys!.Single(fk => fk.Name == subDimension.ForeignKeyName);

                joins.Add(
                    $$"""
                      LEFT JOIN  [{{understanding.Schema!.Name}}].[{{subDimNoun.TableName}}] AS [{{subDimension.Alias}}]
                      ON         [{{dimension.Alias}}].[{{subDimKey.ForeignKeyRef!.ColumnName}}] = 
                                 [{{subDimension.Alias}}].[{{subDimKey.PrimaryKeyRef!.ColumnName}}]
                      """);

                joins.AddRange(GetDimensionalJoins(subDimension, understanding));
            }

            return joins;
        }
    }
}
