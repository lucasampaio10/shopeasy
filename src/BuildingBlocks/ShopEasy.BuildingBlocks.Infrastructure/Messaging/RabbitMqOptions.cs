using System.Text.Json;

namespace ShopEasy.BuildingBlocks.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string User { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string Exchange { get; set; } = "shopeasy.events";

    /// <summary>Queue owned by this service. Every instance of the service competes for its messages.</summary>
    public string Queue { get; set; } = string.Empty;

    public string DeadLetterExchange => $"{Exchange}.dead";

    public string DeadLetterQueue => $"{Queue}.dead";
}

internal static class MessageSerialization
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
