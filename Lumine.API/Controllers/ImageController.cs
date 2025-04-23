using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lumine.API.Controllers
{
    /// <summary>
    /// Controller for handling image-related operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageController"/> class.
        /// </summary>
        /// <param name="imageService">The image service.</param>
        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        /// <summary>
        /// Uploads an image file.
        /// </summary>
        /// <param name="file">The image file to upload.</param>
        /// <returns>The URL of the uploaded image.</returns>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var imageUrl = await _imageService.UploadImageAsync(stream, file.FileName);

            return Ok(new { Url = imageUrl });
        }

        /// <summary>
        /// Deletes an image from Cloudinary.
        /// </summary>
        /// <param name="publicId">The public ID of the image.</param>
        /// <returns>True if deletion was successful.</returns>
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] string? publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return BadRequest("Public ID is required.");

            var success = await _imageService.DeleteImageAsync(publicId);

            if (!success)
                return StatusCode(500, "Failed to delete image.");

            return Ok(new { Message = "Image deleted successfully." });
        }
    }
}
