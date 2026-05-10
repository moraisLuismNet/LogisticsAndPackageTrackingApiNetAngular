using LogisticPackageTrackingApiNet.Application.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace LogisticPackageTrackingApiNet.Infrastructure.Messaging;

public class RabbitMQProducer : IMessagePublisher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQProducer> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMQProducer(IConfiguration configuration, ILogger<RabbitMQProducer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, string queueName)
    {
        await EnsureConnectedAsync();

        if (_channel == null)
        {
            throw new InvalidOperationException("RabbitMQ is not available");
        }

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
        _logger.LogDebug("Published message to queue {Queue}", queueName);
    }

    private async Task EnsureConnectedAsync()
    {
        if (_channel != null) return;

        await _semaphore.WaitAsync();
        try
        {
            if (_channel != null) return;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(queue: "email_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.QueueDeclareAsync(queue: "location_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to RabbitMQ");
            _channel = null;
            _connection = null;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
