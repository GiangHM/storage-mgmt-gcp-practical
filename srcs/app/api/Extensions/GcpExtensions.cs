using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace storageapi.Infra.gcp;

internal static class GcpExtensions
{
    /// <summary>
    /// Registers GCP options from the "GCP" configuration section, wires up validation,
    /// and ensures the application fails fast at startup if any required value is missing
    /// or does not meet the GCP naming constraints.
    /// </summary>
    /// <remarks>
    /// Call <see cref="LogGcpConfiguration"/> after <c>builder.Build()</c> to emit a
    /// one-time structured log of the resolved (non-secret) values for observability.
    /// </remarks>
    public static IServiceCollection AddGcpConfiguration(this IServiceCollection services)
    {
        var optionsBuilder = services
            .AddOptions<GcpOptions>()
            .BindConfiguration(GcpOptions.SectionName);

        // Register custom validation for data annotations
        optionsBuilder.Validate(options =>
        {
            var context = new ValidationContext(options);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(options, context, results, validateAllProperties: true);
        }, "GCP configuration validation failed");

        // Register validator for environment-specific checks and ValidateOnStart
        optionsBuilder.ValidateOnStart();

        // IValidateOptions<T> implementations registered as singletons are automatically
        // discovered by the options infrastructure and run during ValidateOnStart().
        services.AddSingleton<IValidateOptions<GcpOptions>, GcpOptionsValidator>();

        return services;
    }

    /// <summary>
    /// Emits a one-time structured log of the resolved GCP configuration values.
    /// Call this after <c>builder.Build()</c> so that <c>ValidateOnStart</c> has already
    /// confirmed the options are valid before they are logged.
    /// None of the logged properties are secrets; credentials are supplied via
    /// GOOGLE_APPLICATION_CREDENTIALS and never stored in <see cref="GcpOptions"/>.
    /// </summary>
    public static void LogGcpConfiguration(this IServiceProvider services)
    {
        var options = services.GetRequiredService<IOptions<GcpOptions>>().Value;
        var logger = services.GetRequiredService<ILoggerFactory>()
                             .CreateLogger(nameof(GcpOptions));

        logger.LogInformation(
            "GCP configuration: ProjectId={ProjectId}, StorageBucket={StorageBucket}, " +
            "PubSubTopic={PubSubTopic}, FirestoreDatabase={FirestoreDatabase}",
            options.ProjectId,
            options.StorageBucket,
            options.PubSubTopic,
            options.FirestoreDatabase);
    }
}
