using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace storageapi.Infra.gcp;

/// <summary>
/// Validates environment-specific GCP settings that cannot be expressed as data annotations.
/// In Development: verifies that GOOGLE_APPLICATION_CREDENTIALS is set and the file exists,
/// so credential problems are caught at startup rather than on the first SDK call.
/// This check is skipped in non-Development environments because production deployments
/// rely on Workload Identity (Cloud Run) rather than a key file.
/// </summary>
internal sealed class GcpOptionsValidator : IValidateOptions<GcpOptions>
{
    private readonly IHostEnvironment _environment;

    public GcpOptionsValidator(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public ValidateOptionsResult Validate(string? name, GcpOptions options)
    {
        if (!_environment.IsDevelopment() || bool.TryParse(Environment.GetEnvironmentVariable("USE_EMULATOR"), out var useEmulator) && useEmulator)
        {
            return ValidateOptionsResult.Success;
        }

        var credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

        if (string.IsNullOrWhiteSpace(credentialsPath))
        {
            return ValidateOptionsResult.Fail(
                "GOOGLE_APPLICATION_CREDENTIALS is not set. " +
                "For local development, point this environment variable to your service account key file. " +
                "See launchSettings.json or the project README for setup instructions.");
        }

        if (!File.Exists(credentialsPath))
        {
            return ValidateOptionsResult.Fail(
                $"GOOGLE_APPLICATION_CREDENTIALS points to '{credentialsPath}', which does not exist. " +
                "Ensure the service account key file is present at that path before starting the application.");
        }

        return ValidateOptionsResult.Success;
    }
}
