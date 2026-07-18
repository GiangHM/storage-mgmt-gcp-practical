using AutoMapper;
using storageapi.FirestoreEntities;
using StorageManagementAPI.Models;
using System.Collections.Generic;

namespace StorageManagementAPI;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ── Request model → Firestore document ──────────────────────────────────
        // DocTypeCode becomes the Firestore document ID.
        // Timestamps (CreatedAt / UpdatedAt) are set by the service layer, not here.
        CreateMap<DocTypeRequestModel, DocumentTypeDocument>()
            .ForMember(dest => dest.Id,          opt => opt.MapFrom(src => src.DocTypeCode))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DocTypeDescription))
            .ForMember(dest => dest.IsActive,    opt => opt.Ignore())   // default false; caller sets if needed
            .ForMember(dest => dest.CreatedAt,   opt => opt.Ignore())   // set in DocTypeFirestoreService
            .ForMember(dest => dest.UpdatedAt,   opt => opt.Ignore());  // set in DocTypeFirestoreService

        // ── Firestore document → Response model ─────────────────────────────────
        // Timestamp fields are internal; the public API only surfaces the three DTO fields.
        CreateMap<DocumentTypeDocument, DocTypeResponseModel>()
            .ForMember(dest => dest.DocTypeCode,        opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DocTypeDescription, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.IsActive,           opt => opt.MapFrom(src => src.IsActive));
    }
}
