using System.Threading;
using System.Threading.Tasks;

namespace storageapi.Services;

/// <summary>
/// Publishes messages to a Google Cloud Pub/Sub topic.
/// Replaces the Azure Service Bus–based IAzureServiceBusHelper.
/// </summary>
public interface IPubSubPublisher
{
    /// <summary>
    /// Serialises <paramref name="message"/> as JSON and publishes it to the
    /// configured Pub/Sub topic. The event type and a UTC timestamp are added
    /// as message attributes for downstream routing and filtering.
    /// </summary>
    Task PublishMessageAsync<T>(T message, CancellationToken cancellationToken = default);
}
