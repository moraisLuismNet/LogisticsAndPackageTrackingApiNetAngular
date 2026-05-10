namespace LogisticPackageTrackingApiNet.Application.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string queueName);
}
