using Marathon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marathon.Controllers
{
    /// <summary>
    /// Uploads supporting media to Azure Blob Storage. Organisers upload event
    /// banners; any authenticated user can upload a profile picture.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private const long MaxFileBytes = 5 * 1024 * 1024; // 5 MB

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/gif"
        };

        private readonly IBlobService _blobService;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IBlobService blobService, ILogger<UploadController> logger)
        {
            _blobService = blobService;
            _logger = logger;
        }

        /// <summary>Uploads an event banner image (Organisers and Admins only).</summary>
        [Authorize(Roles = "Organiser, Admin")]
        [HttpPost("event-banner")]
        [RequestSizeLimit(MaxFileBytes)]
        public Task<IActionResult> UploadEventBanner(IFormFile file) => UploadAsync(file);

        /// <summary>Uploads a profile picture for the signed-in user.</summary>
        [HttpPost("profile-picture")]
        [RequestSizeLimit(MaxFileBytes)]
        public Task<IActionResult> UploadProfilePicture(IFormFile file) => UploadAsync(file);

        private async Task<IActionResult> UploadAsync(IFormFile? file)
        {
            if (!_blobService.IsConfigured)
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    "Azure Blob Storage is not configured on the server.");

            if (file is null || file.Length == 0)
                return BadRequest("Please select a file to upload.");

            if (file.Length > MaxFileBytes)
                return BadRequest("The file must be 5 MB or smaller.");

            if (!AllowedContentTypes.Contains(file.ContentType))
                return BadRequest("Only JPEG, PNG, WEBP and GIF images are allowed.");

            try
            {
                await using var stream = file.OpenReadStream();
                var url = await _blobService.UploadAsync(stream, file.FileName, file.ContentType);

                return Ok(new { url });
            }
            catch (Exception ex)
            {
                // Keep a storage outage from surfacing as an unhandled 500.
                _logger.LogError(ex, "Uploading {FileName} to blob storage failed.", file.FileName);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    "Image storage is currently unreachable. Please try again later.");
            }
        }
    }
}
