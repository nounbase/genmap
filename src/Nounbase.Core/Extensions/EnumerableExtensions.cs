namespace Nounbase.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool None<T>(this IEnumerable<T> source) =>
            !source.Any();

        public static bool None<T>(this IEnumerable<T> source, Func<T, bool> condition) =>
            !source.Any(condition);

        public static string And(this IEnumerable<string> source) =>
            source.Count() switch
            {
                0 => string.Empty,
                1 => source.First(),
                _ => $"{string.Join(", ", source.Take(source.Count() - 1))} and {source.Last()}"
            };

        public static string Or(this IEnumerable<string> source) =>
            source.Count() switch
            {
                0 => string.Empty,
                1 => source.First(),
                _ => $"{string.Join(", ", source.Take(source.Count() - 1))} or {source.Last()}"
            };
    }
}
