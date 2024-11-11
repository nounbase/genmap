namespace Nounbase.Core.Interfaces.Clients
{
    public interface IChatGptClient
    {
        Task<string?> Complete(string prompt, string? modelName = null);
        Task<T?> Complete<T>(string prompt, string? modelName = null);
    }
}
