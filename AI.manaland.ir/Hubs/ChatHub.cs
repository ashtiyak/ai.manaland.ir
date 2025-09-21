namespace AI.manaland.ir.Hubs;

using AI.manaland.ir.Services;
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    private readonly IMultiAIChatService _chatService;

    public ChatHub(IMultiAIChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task SendMessage(string prompt, string model)
    {
        try
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "user", prompt);
            await Clients.Caller.SendAsync("ShowTyping");
            await foreach (var chunk in _chatService.GetStreamingResponseAsync(prompt, model))
            {
                if (!string.IsNullOrEmpty(chunk))
                {
                    await Clients.Caller.SendAsync("ReceiveMessageChunk", chunk);
                }
            }
            await Clients.Caller.SendAsync("HideTyping");
            Console.WriteLine("Kamel Baye. Elam code MAKELESH");
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", $"خطا: {ex.Message}");
            Console.WriteLine($"Amo Khata Heda: ChatHub: {ex}");
        }
    }
}