using Microsoft.Extensions.DependencyInjection;
using storageapi.Services;

namespace storageapi.Extensions;

/// <summary>
/// DI registration helpers for the Google Cloud Pub/Sub publisher.
/// </summary>
public static class PubSubExtensions
{
    /// <summary>
    /// Registers <see cref="PubSubPublisher"/> as a singleton that is also wired into the
    /// ASP.NET Core hosted-service pipeline.
    ///
    /// Registration strategy:
    ///   • A single <see cref="PubSubPublisher"/> instance is created (singleton lifetime).
    ///   • <see cref="IPubSubPublisher"/> resolves to that same instance for injection into controllers/services.
    ///   • <see cref="Microsoft.Extensions.Hosting.IHostedService"/> resolves to the same instance so
    ///     <c>StartAsync</c> / <c>StopAsync</c> are called by the host, ensuring the Pub/Sub channel is
    ///     open before requests arrive and flushed before the process exits.
    /// </summary>
    public static IServiceCollection AddPubSubPublisher(this IServiceCollection services)
    {
        // Concrete singleton — resolved by both interface registrations below.
        services.AddSingleton<PubSubPublisher>();

        // Interface alias → same instance.
        services.AddSingleton<IPubSubPublisher>(
            sp => sp.GetRequiredService<PubSubPublisher>());

        // Hook into host lifetime for StartAsync / StopAsync.
        services.AddHostedService(
            sp => sp.GetRequiredService<PubSubPublisher>());

        return services;
    }
}
