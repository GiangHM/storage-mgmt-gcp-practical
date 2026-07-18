using System.ComponentModel.DataAnnotations;

namespace storageapi.Infra.gcp;

/// <summary>
/// Strongly-typed binding for the "GCP" configuration section.
/// All properties are required; <see cref="ProjectId"/> is additionally
/// constrained to the GCP project-ID naming rules.
/// </summary>
public sealed class GcpOptions
{
    /// <summary>The configuration section key this class binds to.</summary>
    internal const string SectionName = "GCP";

    /// <summary>
    /// GCP project identifier.
    /// Rules: lowercase letters, digits, and hyphens; 6–30 characters;
    /// must start with a lowercase letter and end with a lowercase letter or digit.
    /// </summary>
    [Required(ErrorMessage = "GCP ProjectId is required.")]
    [RegularExpression(
        @"^[a-z][a-z0-9\-]{4,28}[a-z0-9]$",
        ErrorMessage =
            "GCP ProjectId must be 6–30 characters, start with a lowercase letter, " +
            "end with a lowercase letter or digit, and contain only lowercase letters, digits, and hyphens.")]
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Cloud Storage bucket name for document storage.</summary>
    [Required(ErrorMessage = "GCP StorageBucket is required.")]
    public string StorageBucket { get; set; } = string.Empty;

    public int SignedUrlExpirationMinutes { get; set; } = 15;

    /// <summary>Pub/Sub topic name for document-change events.</summary>
    [Required(ErrorMessage = "GCP PubSubTopic is required.")]
    public string PubSubTopic { get; set; } = string.Empty;

    /// <summary>
    /// Firestore database identifier. Use "(default)" for the default database.
    /// </summary>
    [Required(ErrorMessage = "GCP FirestoreDatabase is required.")]
    public string FirestoreDatabase { get; set; } = string.Empty;
}
