using Nounbase.Core.Models.Schema;
using Nounbase.Core.Models.Semantic;

namespace Nounbase.Core.Extensions
{
    public static class TableListExtensions
    {
        public static Table ByNoun(this IList<Table> tables, Noun noun) =>
            tables.First(t => t.Name == noun.TableName);
    }
}
