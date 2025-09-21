namespace AI.manaland.ir.Services;

using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

public class ArvanAIProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public ArvanAIProvider(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config["ArvanAI:ApiKey"]}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<string> GetResponseAsync(string prompt, string model)
    {
        model = model ?? _config["ArvanAI:Model"] ?? throw new ArgumentNullException(nameof(model));
        var url = _config["ArvanAI:Endpoint"] + "/chat/completions";
        var requestBody = new
        {
            model = model,
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 1000 // افزایش قابل‌توجه برای متن طولانی‌تر
        };

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"خطای API آروان: {response.StatusCode} - {error}");
        }

        var result = await response.Content.ReadAsStringAsync();
        var parsed = JObject.Parse(result);
        return parsed["choices"]?[0]?["message"]?["content"]?.ToString() ?? "خطا در دریافت پاسخ.";
    }

    public async IAsyncEnumerable<string> GetStreamingResponseAsync(string prompt, string model)
    {
        model = model ?? _config["ArvanAI:Model"] ?? throw new ArgumentNullException(nameof(model));
        var url = _config["ArvanAI:Endpoint"] + "/chat/completions";
        var requestBody = new
        {
            model = model,
            messages = new[] { new { role = "user", content = prompt } },
            max_tokens = 1000, // افزایش برای متن طولانی‌تر
            stream = true
        };

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"خطای API آروان: {response.StatusCode} - {error}");
        }

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        int maxIterations = 5000; // افزایش قابل‌توجه برای پوشش متن طولانی
        int iteration = 0;
        bool isDone = false;
        while (!reader.EndOfStream && iteration < maxIterations && !isDone)
        {
            var line = await reader.ReadLineAsync();
            if (line == null) break;
            if (line.StartsWith("data: "))
            {
                var jsonPart = line.Substring(6).Trim();
                if (jsonPart == "[DONE]")
                {
                    isDone = true;
                    break;
                }
                var delta = ParseJsonChunk(jsonPart);
                if (!string.IsNullOrEmpty(delta))
                    yield return delta;
            }
            iteration++;
        }
        if (iteration >= maxIterations)
        {
            Console.WriteLine("هشدار: حداکثر تعداد تکرار در استریم reached. ممکن است متن ناقص باشد.");
        }
        else
        {
            Console.WriteLine("استریم با موفقیت تکمیل شد.");
        }
    }

    private string? ParseJsonChunk(string jsonPart)
    {
        try
        {
            var parsed = JObject.Parse(jsonPart);
            return parsed["choices"]?[0]?["delta"]?["content"]?.ToString() ?? "";
        }
        catch
        {
            return null;
        }
    }
}