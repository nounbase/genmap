namespace Nounbase.Core.Utilities
{
    public static class Environment
    {
        public static string GetRequiredEnvironmentVariable(string name) =>
            System.Environment.GetEnvironmentVariable(name ?? throw new ArgumentNullException(nameof(name)))
            ?? throw new InvalidOperationException($"[{name}] environment variable not configured.");
    }
}
