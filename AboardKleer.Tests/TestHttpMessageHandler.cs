using System.Collections;

public class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new();
    public List<HttpRequestMessage> SentRequests { get; } = new();

    public void EnqueueResponse(HttpResponseMessage response) =>
         _responses.Enqueue(response);
    
    public void EnqueueResponse(string content, string mediaType="application/json")
    {
        _responses.Enqueue(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(content, System.Text.Encoding.UTF8, mediaType)
        });
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        SentRequests.Add(request);
        return Task.FromResult(_responses.Dequeue());
    }
}