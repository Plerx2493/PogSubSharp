using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PogSubSharp.Shared.Notifications;
using PogSubSharp.Shared.Transport;
using PogSubSharp.Subscriptions;

namespace PogSubSharp.Clients;

public partial class EventSubClient
{
    private bool isCanceled = false;
    
    private ILogger _logger;
    public string SessionId { get; private set; } = string.Empty;
    public readonly NotificationHandler NotificationHandler = new();
    public readonly EventSubClientConfiguration Configuration;
    public readonly SubscriptionManager Subscriptions;
    public readonly IEventSubTransport Transport; 

    private readonly HttpClient _httpClient = new();

    public EventSubClient(ILogger<EventSubClient> logger, EventSubClientConfiguration configuration, IEventSubTransport transport)
    {
        Configuration = configuration;
        Transport = transport;
        Subscriptions = new SubscriptionManager(this);
        _logger = logger;
        
    }
    
    public async Task<SubscriptionCreateResponse> CreateSubscriptionAsync(SubscriptionCreatePayload payload, string oauthToken)
    {
        if (isCanceled)
        {
            throw new InvalidOperationException("The client has been canceled and can no longer be used.");
        }

        if (string.IsNullOrEmpty(SessionId))
        {
            throw new InvalidOperationException("The client is not connected to the EventSub WebSocket.");
        }

        string json = JsonSerializer.Serialize(payload);

        using HttpRequestMessage request = new(HttpMethod.Post, "https://api.twitch.tv/helix/eventsub/subscriptions");
        request.Headers.Add("Client-ID", Configuration.ClientId);
        request.Headers.Add("Authorization", $"Bearer {oauthToken}");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage res = await _httpClient.SendAsync(request);

        switch (res.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                string content = await res.Content.ReadAsStringAsync();
                throw new InvalidOperationException("The provided OAuth token is not authorized to create subscriptions.");
            case HttpStatusCode.BadRequest:
                //string content = await res.Content.ReadAsStringAsync();
                //throw new InvalidOperationException($"The request was malformed: {content}");
            case HttpStatusCode.Forbidden:
                throw new InvalidOperationException("The client is not authorized to create subscriptions.");
            case HttpStatusCode.Conflict:
                throw new InvalidOperationException("The subscription already exists.");
            case HttpStatusCode.TooManyRequests:
                throw new InvalidOperationException("The request exceeds the number of subscriptions that you may create with the same combination of type and condition values");
            case HttpStatusCode.InternalServerError:
                throw new InvalidOperationException("The server encountered an error while processing the request.");
        }
        
        SubscriptionCreateResponse? response = await res.Content.ReadFromJsonAsync<SubscriptionCreateResponse>();
        
        if (response is null)
        {
            throw new InvalidOperationException("The server did not return a valid response.");
        }
        
        _logger.LogInformation("Created subscription: {Content}", response);
        return response;
    }

    public async Task DeleteSubscriptionAsync(string id, string oauthToken)
    {
        if (isCanceled)
        {
            throw new InvalidOperationException("The client has been canceled and can no longer be used.");
        }

        using HttpRequestMessage request = new(HttpMethod.Delete,
            $"https://api.twitch.tv/helix/eventsub/subscriptions?id={id}");
        request.Headers.Add("Client-ID", Configuration.ClientId);
        request.Headers.Add("Authorization", $"Bearer {oauthToken}");

        HttpResponseMessage res = await _httpClient.SendAsync(request);
        
        switch (res.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                string content = await res.Content.ReadAsStringAsync();
                throw new InvalidOperationException("The provided OAuth token is not authorized to delete subscriptions.");
            case HttpStatusCode.NotFound:
                throw new InvalidOperationException("The subscription does not exist.");
            case HttpStatusCode.Forbidden:
                throw new InvalidOperationException("The client is not authorized to delete subscriptions.");
            case HttpStatusCode.InternalServerError:
                throw new InvalidOperationException("The server encountered an error while processing the request.");
        }
        
        _logger.LogInformation("Deleted subscription with id {Id}", id);
    }
}