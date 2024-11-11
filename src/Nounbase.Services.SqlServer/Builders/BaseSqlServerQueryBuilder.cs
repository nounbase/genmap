using Nounbase.Core.Models.Semantic;
using Nounbase.Core.Models.Semantic.Relational;

namespace Nounbase.Services.SqlServer.Builders
{
    public abstract class BaseSqlServerQueryBuilder
    {
        protected IEnumerable<string> GetSelectableColumns(Noun noun) =>
           noun.Properties.Select(p => GetSelectableColumnClause(noun, p));

        protected IEnumerable<string> GetSelectableColumns(Branch branch) =>
            branch.Properties.Select(p => GetSelectableColumnClause(branch, p));

        protected string GetSelectableColumnClause(Noun noun, Property property) =>
            property.SourceAlias == null
                ? $"[{noun.TableName}].[{property.Name}] AS [{property.Name}]"
                : $"[{property.SourceAlias}].[{property.Name}] AS [{property.SourceAlias}_{property.Name}]";

        protected string GetSelectableColumnClause(Branch branch, Property property) =>
            property.SourceAlias == null
                ? $"[{branch.TableName}].[{property.Name}] AS [{property.Name}]"
                : $"[{property.SourceAlias}].[{property.Name}] AS [{property.SourceAlias}_{property.Name}]";
    }
}
