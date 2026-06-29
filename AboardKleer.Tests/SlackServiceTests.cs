using AboardKleerIntegration.Services.Slack;
using AboardKleerIntegration.Config;
using Microsoft.Extensions.Options;

namespace AboardKleerIntegration.Tests;

[TestClass]
public class SlackServiceTests
{
    private SlackService _slackService = null!;
    private TestHttpMessageHandler _httpMessageHandler = null!;
    private HttpClient _httpClient = null!;

    [TestInitialize]
    public void Setup()
    {
        _httpMessageHandler = new TestHttpMessageHandler();
        _httpClient = new HttpClient(_httpMessageHandler);
        _slackService = new SlackService(_httpClient, Options.Create(
            new SlackOptions { WebhookUrl = "https://hooks.slack.com/test-webhook" }));
    }

    [TestMethod]
    public async Task SendMessageAsync_PostsJsonToWebhookUrl()
    {

        _httpMessageHandler.EnqueueResponse("ok");

        await _slackService.SendMessageAsync("Hello Slack!");

        var sentRequest = _httpMessageHandler.SentRequests.Single();
        Assert.AreEqual(HttpMethod.Post, sentRequest.Method);
        Assert.AreEqual("https://hooks.slack.com/test-webhook", sentRequest.RequestUri!.ToString());

        var content = await sentRequest.Content!.ReadAsStringAsync();
        Assert.Contains("\"text\":\"Hello Slack!\"", content);
    }
}