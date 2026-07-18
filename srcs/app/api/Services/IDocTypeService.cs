using storageapi.FirestoreEntities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StorageManagementAPI.Services;

/// <summary>
/// CRUD operations for document types backed by Cloud Firestore.
/// Replaces the Azure Table Storage–based IDocTypeTableService.
/// </summary>
public interface IDocTypeService
{
    /// <summary>Returns all document-type documents from Firestore.</summary>
    Task<IEnumerable<DocumentTypeDocument>> GetAllDocumentTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the document type with the given <paramref name="id"/>, or
    /// <see langword="null"/> when no such document exists.
    /// </summary>
    Task<DocumentTypeDocument?> GetDocumentTypeAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new document type. <see cref="DocumentTypeDocument.Id"/> is used
    /// as the Firestore document ID and must be set before calling this method.
    /// </summary>
    Task<DocumentTypeDocument> CreateDocumentTypeAsync(DocumentTypeDocument document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing document type identified by <paramref name="id"/>.
    /// Returns <see langword="null"/> if the document does not exist.
    /// </summary>
    Task<DocumentTypeDocument?> UpdateDocumentTypeAsync(string id, DocumentTypeDocument document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the document type with the given <paramref name="id"/>.
    /// Returns <see langword="false"/> if the document did not exist.
    /// </summary>
    Task<bool> DeleteDocumentTypeAsync(string id, CancellationToken cancellationToken = default);
}
