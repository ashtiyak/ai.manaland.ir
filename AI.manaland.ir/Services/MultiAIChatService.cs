namespace AI.manaland.ir.Services;

public class MultiAIChatService : IMultiAIChatService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;

    public MultiAIChatService(IServiceProvider serviceProvider, IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        _config = config;
    }

    public async Task<string> GetResponseAsync(string prompt, string providerName)
    {
        Console.WriteLine($"درخواست برای مدل: {providerName}");
        using var scope = _serviceProvider.CreateScope();
        var providers = new Dictionary<string, Func<IAIProvider>>
        {
            { "ArvanAI", () => scope.ServiceProvider.GetRequiredService<IAIProvider>() }
        };
        if (!providers.TryGetValue(providerName, out var factory))
        {
            Console.WriteLine($"خطا: مدل {providerName} پشتیبانی نمی‌شود.");
            throw new ArgumentException($"مدل {providerName} پشتیبانی نمی‌شود.");
        }
        var provider = factory();
#pragma warning disable CS8604 // Possible null reference argument.
        return await provider.GetResponseAsync(prompt, _config[$"{providerName}:Model"]);
#pragma warning restore CS8604 // Possible null reference argument.
    }

    public async IAsyncEnumerable<string> GetStreamingResponseAsync(string prompt, string providerName)
    {
        Console.WriteLine($"درخواست استریم برای مدل: {providerName}");
        using var scope = _serviceProvider.CreateScope();
        var providers = new Dictionary<string, Func<IAIProvider>>
        {
            { "ArvanAI", () => scope.ServiceProvider.GetRequiredService<IAIProvider>() }
        };
        if (!providers.TryGetValue(providerName, out var factory))
        {
            Console.WriteLine($"خطا: مدل {providerName} پشتیبانی نمی‌شود.");
            throw new ArgumentException($"مدل {providerName} پشتیبانی نمی‌شود.");
        }
        var provider = factory();
#pragma warning disable CS8604 // Possible null reference argument.
        await foreach (var chunk in provider.GetStreamingResponseAsync(prompt, _config[$"{providerName}:Model"]))
        {
            yield return chunk;
        }
#pragma warning restore CS8604 // Possible null reference argument.
    }
}