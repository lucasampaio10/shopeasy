using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ShopEasy.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Single long-lived connection per process; channels are cheap and created on demand.</summary>
internal sealed class RabbitMqConnection(IOptions<RabbitMqOptions> options) : IDisposable
{
    private readonly object _gate = new();
    private readonly ConnectionFactory _factory = new()
    {
        HostName = options.Value.Host,
        Port = options.Value.Port,
        UserName = options.Value.User,
        Password = options.Value.Password,
        DispatchConsumersAsync = true,
        AutomaticRecoveryEnabled = true,
    };

    private IConnection? _connection;

    public IModel CreateChannel()
    {
        lock (_gate)
        {
            if (_connection is not { IsOpen: true })
            {
                _connection?.Dispose();
                _connection = _factory.CreateConnection();
            }

            return _connection.CreateModel();
        }
    }

    public void Dispose() => _connection?.Dispose();
}
