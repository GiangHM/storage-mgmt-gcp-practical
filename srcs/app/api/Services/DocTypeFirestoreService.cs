using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using storageapi.FirestoreEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StorageManagementAPI.Services;

/// <summary>
/// Firestore-backed implementation of <see cref="IDocTypeService"/>.
/// All documents are stored in the "document-types" collection.
/// FirestoreDb is injected as a singleton; this service is scoped.
/// </summary>
internal sealed class DocTypeFirestoreService(
    FirestoreDb firestoreDb,
    ILogger<DocTypeFirestoreService> logger) : IDocTypeService
{
    private const string CollectionName = "document-types";

    // Resolved once so every method avoids the string allocation.
    private readonly CollectionReference _collection = firestoreDb.Collection(CollectionName);

    /// <inheritdoc/>
    public async Task<IEnumerable<DocumentTypeDocument>> GetAllDocumentTypesAsync(
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Fetching all document types from Firestore collection '{Collection}'", CollectionName);

        var snapshot = await _collection.GetSnapshotAsync(cancellationToken).ConfigureAwait(false);

        return snapshot.Documents
            .Where(d => d.Exists)
            .Select(MapSnapshot);
    }

    /// <inheritdoc/>
    public async Task<DocumentTypeDocument?> GetDocumentTypeAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        logger.LogInformation("Fetching document type '{Id}' from Firestore", id);

        var snapshot = await _collection.Document(id)
            .GetSnapshotAsync(cancellationToken)
            .ConfigureAwait(false);

        return snapshot.Exists ? MapSnapshot(snapshot) : null;
    }

    /// <inheritdoc/>
    public async Task<DocumentTypeDocument> CreateDocumentTypeAsync(
        DocumentTypeDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(document.Id, nameof(document.Id));

        var now = Timestamp.FromDateTime(DateTime.UtcNow);
        document.CreatedAt = now;
        document.UpdatedAt = now;

        logger.LogInformation("Creating document type '{Id}' in Firestore", document.Id);

        await _collection.Document(document.Id)
            .SetAsync(document, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation("Document type '{Id}' created successfully", document.Id);
        return document;
    }

    /// <inheritdoc/>
    public async Task<DocumentTypeDocument?> UpdateDocumentTypeAsync(
        string id,
        DocumentTypeDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(document);

        var docRef = _collection.Document(id);
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken).ConfigureAwait(false);

        if (!snapshot.Exists)
        {
            logger.LogWarning("Document type '{Id}' not found; update skipped", id);
            return null;
        }

        // Preserve the original createdAt; only bump updatedAt.
        var existing = MapSnapshot(snapshot);
        document.Id = id;
        document.CreatedAt = existing.CreatedAt;
        document.UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow);

        logger.LogInformation("Updating document type '{Id}' in Firestore", id);

        await docRef.SetAsync(document, cancellationToken: cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Document type '{Id}' updated successfully", id);
        return document;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteDocumentTypeAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var docRef = _collection.Document(id);
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken).ConfigureAwait(false);

        if (!snapshot.Exists)
        {
            logger.LogWarning("Document type '{Id}' not found; delete skipped", id);
            return false;
        }

        logger.LogInformation("Deleting document type '{Id}' from Firestore", id);
        await docRef.DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Document type '{Id}' deleted successfully", id);
        return true;
    }

    // Converts a Firestore DocumentSnapshot to DocumentTypeDocument,
    // injecting the document ID which is not stored as a Firestore field.
    private static DocumentTypeDocument MapSnapshot(DocumentSnapshot snapshot)
    {
        var doc = snapshot.ConvertTo<DocumentTypeDocument>();
        doc.Id = snapshot.Id;
        return doc;
    }
}
