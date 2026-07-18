using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using storageapi.Models;
using StorageManagementAPI.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StorageManagementAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignedUrlGeneratorController(
        ILogger<SignedUrlGeneratorController> _logger,
        ICloudStorageService _cloudStorageService) : ControllerBase
    {
        [HttpPost("generate")]
        [ProducesResponseType(typeof(SignedUrlResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SignedUrlResponseModel>> GenerateSignedUrlAsync(
            [FromBody] SignedUrlRequestModel request,
            CancellationToken cancellationToken)
        {
            try
            {
                var objectName = $"{request.FileName}";
                _logger.LogInformation("Generating signed URL for {ObjectName}", objectName);

                var uploadUrl = await _cloudStorageService.GenerateSignedUrlAsync(objectName, cancellationToken);

                return Ok(new SignedUrlResponseModel
                {
                    UploadUrl = uploadUrl,
                    ObjectName = objectName,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate signed URL for file {FileName}", request.FileName);
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to generate signed URL.");
            }
        }
    }
}
