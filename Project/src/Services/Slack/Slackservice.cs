using System.Text.Json;
using AboardKleerIntegration.Config;
using Microsoft.Extensions.Options;

namespace AboardKleerIntegration.Services.Slack;
public class SlackService : ISlackService
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;

    public SlackService(HttpClient httpClient, IOptions<SlackOptions> options)
    {
        _httpClient = httpClient;
        _webhookUrl = options.Value.WebhookUrl;
    }

    public async Task SendMessageAsync(string message)
    {
        var payload = new { text = message };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        await _httpClient.PostAsync(_webhookUrl, content);
    }
}