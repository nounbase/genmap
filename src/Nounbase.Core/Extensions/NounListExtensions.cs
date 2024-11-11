using Nounbase.Core.Models.Semantic;

namespace Nounbase.Core.Extensions
{
    public static class NounListExtensions
    {
        public static Noun? ByName(this IList<Noun> nouns, string name) =>
            nouns.FirstOrDefault(n => n.Name! == name);

        public static Noun? ByTableName(this IList<Noun> nouns, string tableName) =>
            nouns.FirstOrDefault(n => n.TableName! == tableName);
    }
}
