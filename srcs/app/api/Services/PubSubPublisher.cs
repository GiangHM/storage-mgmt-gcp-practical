using Google.Api.Gax.Grpc;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using storageapi.Infra.gcp;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace storageapi.Services;

/// <summary>
/// Singleton Pub/Sub publisher that integrates with the ASP.NET Core host lifetime.
/// <see cref="IHostedService.StartAsync"/> initialises the <see cref="PublisherClient"/>
/// (with exponential back-off retry) before the app starts serving requests.
/// <see cref="IHostedService.StopAsync"/> flushes pending messages and shuts down cleanly.
/// </summary>
public sealed class PubSubPublisher : IPubSubPublisher, IHostedService
{
    private readonly ILogger<PubSubPublisher> _logger;
    private readonly string _projectId;
    private readonly string _topicId;

    // Nullable until StartAsync completes; guarded in PublishMessageAsync.
    private PublisherClient? _publisherClient;

    public PubSubPublisher(IOptions<GcpOptions> configuration, ILogger<PubSubPublisher> logger)
    {
        _logger = logger;

        _projectId = configuration.Value.ProjectId;
        if (string.IsNullOrWhiteSpace(_projectId))
            throw new InvalidOperationException(
                "GCP:ProjectId configuration value is missing or empty. " +
                "Set it in appsettings.json or via the GCP__ProjectId environment variable.");

        _topicId = configuration.Value.PubSubTopic;
        if (string.IsNullOrWhiteSpace(_topicId))
            throw new InvalidOperationException(
                "GCP:PubSubTopic configuration value is missing or empty. " +
                "Set it in appsettings.json or via the GCP__PubSubTopic environment variable.");
    }

    /// <summary>
    /// Creates the <see cref="PublisherClient"/> with explicit retry settings.
    /// Runs before the application starts accepting HTTP traffic.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Initialising Pub/Sub publisher for project '{ProjectId}', topic '{TopicId}'",
            _projectId, _topicId);

        var topicName = TopicName.FromProjectTopic(_projectId, _topicId);

        // Apply explicit retry to the underlying Publish RPC so transient failures
        // (quota exhaustion, brief unavailability) are retried before bubbling up.
        var retrySettings = RetrySettings.FromExponentialBackoff(
            maxAttempts: 5,
            initialBackoff: TimeSpan.FromMilliseconds(100),
            maxBackoff: TimeSpan.FromSeconds(60),
            backoffMultiplier: 1.3,
            retryFilter: RetrySettings.FilterForStatusCodes(
                StatusCode.Unavailable,
                StatusCode.ResourceExhausted));

        _publisherClient = await new PublisherClientBuilder
        {
            TopicName = topicName,
            EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOrProduction,
            ApiSettings = new PublisherServiceApiSettings
            {
                PublishSettings = CallSettings.FromRetry(retrySettings)
            }
        }.BuildAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Pub/Sub publisher ready for topic '{TopicId}'", _topicId);
    }

    /// <summary>
    /// Flushes any in-flight messages and shuts down the publisher gracefully.
    /// Allows up to 15 seconds for buffered messages to drain.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_publisherClient is null)
            return;

        _logger.LogInformation("Shutting down Pub/Sub publisher for topic '{TopicId}'", _topicId);

        // ShutdownAsync flushes the internal send buffer before closing the channel.
        await _publisherClient.ShutdownAsync(TimeSpan.FromSeconds(15)).ConfigureAwait(false);

        _logger.LogInformation("Pub/Sub publisher shut down cleanly");
    }

    /// <inheritdoc/>
    public async Task PublishMessageAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (_publisherClient is null)
            throw new InvalidOperationException(
                "Pub/Sub publisher is not initialised. " +
                "Ensure the application host has started before publishing.");

        var body = JsonSerializer.Serialize(message);

        var pubSubMessage = new PubsubMessage
        {
            Data = ByteString.CopyFromUtf8(body),
            // Attributes allow subscribers to filter messages without deserialising the body.
            Attributes =
            {
                ["eventType"] = typeof(T).Name,
                ["timestamp"] = DateTime.UtcNow.ToString("O")   // ISO-8601 round-trip
            }
        };

        try
        {
            _logger.LogInformation(
                "Publishing message of type '{EventType}' to topic '{TopicId}'",
                typeof(T).Name, _topicId);

            var messageId = await _publisherClient.PublishAsync(pubSubMessage).ConfigureAwait(false);

            _logger.LogInformation(
                "Message published. MessageId: '{MessageId}', EventType: '{EventType}'",
                messageId, typeof(T).Name);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            // Topic does not exist – configuration error; fail fast so it's surfaced immediately.
            _logger.LogError(ex,
                "Pub/Sub topic '{TopicId}' not found in project '{ProjectId}'. " +
                "Verify the topic exists and the service account has pubsub.publisher role.",
                _topicId, _projectId);
            throw;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.ResourceExhausted)
        {
            // Quota exhausted after all retries.
            _logger.LogError(ex,
                "Pub/Sub quota exhausted publishing '{EventType}' to '{TopicId}'",
                typeof(T).Name, _topicId);
            throw;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex,
                "gRPC error publishing '{EventType}' to '{TopicId}': {Status}",
                typeof(T).Name, _topicId, ex.Status);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error publishing '{EventType}' to '{TopicId}'",
                typeof(T).Name, _topicId);
            throw;
        }
    }
}
