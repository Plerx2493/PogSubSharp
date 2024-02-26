using System.Text.Json.Serialization;

namespace PogSubSharp.Models.Events;

public class ChannelPointsCustomRewardRedemptionAddEvent : IEventSubEvent
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("broadcaster_user_id")]
    public string BroadcasterUserId { get; set; }

    [JsonPropertyName("broadcaster_user_login")]
    public string BroadcasterUserLogin { get; set; }

    [JsonPropertyName("broadcaster_user_name")]
    public string BroadcasterUserName { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("user_login")]
    public string UserLogin { get; set; }

    [JsonPropertyName("user_name")]
    public string UserName { get; set; }

    [JsonPropertyName("user_input")]
    public string UserInput { get; set; }

    [JsonPropertyName("status")]
    public ChannelPointsRedemptionStatus Status { get; set; }

    [JsonPropertyName("reward")]
    public ChannelPointsReward Reward { get; set; }

    [JsonPropertyName("redeemed_at")]
    public DateTimeOffset RedeemedAt { get; set; }
}

public class ChannelPointsReward
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }

    [JsonPropertyName("cost")]
    public int Cost { get; set; }
}

public enum ChannelPointsRedemptionStatus
{
    unknown,
    fulfilled,
    unfulfilled,
    canceled
}