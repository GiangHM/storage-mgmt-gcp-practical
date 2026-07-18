using Google.Cloud.Firestore;

namespace storageapi.FirestoreEntities;

/// <summary>
/// Firestore document representing a document type.
/// Collection: "document-types". Document ID = DocTypeCode.
/// </summary>
[FirestoreData]
public class DocumentTypeDocument
{
    // Document ID – populated from DocumentSnapshot.Id after reads.
    // NOT stored as a Firestore field; Firestore manages it as the document path.
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("description")]
    public string Description { get; set; } = string.Empty;

    [FirestoreProperty("isActive")]
    public bool IsActive { get; set; }

    /// <summary>UTC timestamp set once on creation.</summary>
    [FirestoreProperty("createdAt")]
    public Timestamp CreatedAt { get; set; }

    /// <summary>UTC timestamp updated on every write.</summary>
    [FirestoreProperty("updatedAt")]
    public Timestamp UpdatedAt { get; set; }
}
