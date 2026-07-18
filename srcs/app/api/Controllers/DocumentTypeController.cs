using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using storageapi.FirestoreEntities;
using StorageManagementAPI.Models;
using StorageManagementAPI.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StorageManagementAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentTypeController(
    ILogger<DocumentTypeController> logger,
    IDocTypeService docTypeService,
    IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<DocTypeResponseModel>> GetAll(CancellationToken cancellationToken)
    {
        logger.LogInformation("Get all document types");
        var documents = await docTypeService.GetAllDocumentTypesAsync(cancellationToken);
        return mapper.Map<IEnumerable<DocTypeResponseModel>>(documents);
    }

    [HttpPost]
    public async Task<bool> CreateNewdocType(DocTypeRequestModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Create new document type '{Code}'", model.DocTypeCode);
        var document = mapper.Map<DocumentTypeDocument>(model);
        await docTypeService.CreateDocumentTypeAsync(document, cancellationToken);
        return true;
    }

    /// <summary>
    /// Returns 404 when the document type code does not exist (improved from the previous
    /// in-memory scan over GetAllData()). Firestore performs a direct document lookup by ID.
    /// </summary>
    [HttpGet("{code}")]
    public async Task<ActionResult<DocTypeResponseModel>> GetByCode(
        [FromRoute] string code,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Get document type by code '{Code}'", code);
        var document = await docTypeService.GetDocumentTypeAsync(code, cancellationToken);

        if (document is null)
            return NotFound();

        return mapper.Map<DocTypeResponseModel>(document);
    }
}
