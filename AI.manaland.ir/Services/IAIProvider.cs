namespace AI.manaland.ir.Services;

public interface IAIProvider
{
    Task<string> GetResponseAsync(string prompt, string model);
    IAsyncEnumerable<string> GetStreamingResponseAsync(string prompt, string model);
}

public interface IMultiAIChatService
{
    Task<string> GetResponseAsync(string prompt, string providerName);
    IAsyncEnumerable<string> GetStreamingResponseAsync(string prompt, string providerName);
}