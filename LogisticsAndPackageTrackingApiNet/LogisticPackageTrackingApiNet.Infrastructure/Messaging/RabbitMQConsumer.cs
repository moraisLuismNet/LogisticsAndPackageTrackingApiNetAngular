using LogisticPackageTrackingApiNet.Application.Interfaces;
using LogisticPackageTrackingApiNet.Application.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace LogisticPackageTrackingApiNet.Infrastructure.Messaging;

public class RabbitMQConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMQConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMQConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory, ILogger<RabbitMQConsumer> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                    Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                    UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                    Password = _configuration["RabbitMQ:Password"] ?? "guest"
                };

                _connection = await factory.CreateConnectionAsync(stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await _channel.QueueDeclareAsync(queue: "email_queue", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
                await _channel.QueueDeclareAsync(queue: "location_queue", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);

                var emailConsumer = new AsyncEventingBasicConsumer(_channel);
                emailConsumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var body = Encoding.UTF8.GetString(ea.Body.Span);
                        var message = JsonSerializer.Deserialize<EmailMessage>(body);
                        if (message != null)
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                            await emailSender.SendEmailAsync(message.To, message.Subject, message.Body);
                        }
                        await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing email message");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken: stoppingToken);
                    }
                };

                var locationConsumer = new AsyncEventingBasicConsumer(_channel);
                locationConsumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var body = Encoding.UTF8.GetString(ea.Body.Span);
                        var message = JsonSerializer.Deserialize<LocationUpdateMessage>(body);
                        if (message != null)
                        {
                            _logger.LogInformation("Location update: {TrackingNumber} at ({Lat},{Lng}) - {Status}",
                                message.TrackingNumber, message.Latitude, message.Longitude, message.Status);
                        }
                        await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing location message");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken: stoppingToken);
                    }
                };

                await _channel.BasicConsumeAsync("email_queue", autoAck: false, consumer: emailConsumer, cancellationToken: stoppingToken);
                await _channel.BasicConsumeAsync("location_queue", autoAck: false, consumer: locationConsumer, cancellationToken: stoppingToken);

                _logger.LogInformation("RabbitMQ consumer connected and listening");
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ connection failed, retrying in 10 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    public override void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _connection?.CloseAsync().GetAwaiter().GetResult();
        base.Dispose();
    }
}
