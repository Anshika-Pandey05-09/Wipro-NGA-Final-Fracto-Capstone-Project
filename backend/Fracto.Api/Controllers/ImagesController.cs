// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;

// namespace Fracto.Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class ImagesController : ControllerBase
//     {
//         private readonly IWebHostEnvironment _env;
//         public ImagesController(IWebHostEnvironment env) => _env = env;

//         // POST: /api/images/upload
//         [HttpPost("upload")]
//         [Authorize(Roles = "Admin")]
//         [RequestSizeLimit(10_000_000)]
//         [Consumes("multipart/form-data")]
//         public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken ct)
//         {
//             if (file == null || file.Length == 0)
//                 return BadRequest("No file uploaded.");

//             var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
//             var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
//             if (!allowed.Contains(ext))
//                 return BadRequest("Unsupported image type.");

//             var wwwroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
//             var uploadsDir = Path.Combine(wwwroot, "uploads");
//             Directory.CreateDirectory(uploadsDir);

//             var fileName = $"{Guid.NewGuid():N}{ext}";
//             var fullPath = Path.Combine(uploadsDir, fileName);

//             await using (var stream = System.IO.File.Create(fullPath))
//             {
//                 await file.CopyToAsync(stream, ct);
//             }

//             var path = $"/uploads/{fileName}";
//             return Ok(new { path, fileName });
//         }
//     }
// }

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fracto.Api.Controllers
{
    // DTO to describe multipart/form-data for Swagger & model binding
    public class ImageUploadForm
    {
        /// <summary>The image file to upload.</summary>
        public IFormFile File { get; set; } = default!;
        /// <summary>Optional caption.</summary>
        public string? Caption { get; set; }
    }

    public class ImageUploadResult
    {
        public string Path { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public long Size { get; set; }
        public string? ContentType { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        public ImagesController(IWebHostEnvironment env) => _env = env;

        // POST: /api/images/upload
        [HttpPost("upload")]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(10_000_000)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImageUploadResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload([FromForm] ImageUploadForm form, CancellationToken ct)
        {
            var file = form.File;
            if (file is null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                return BadRequest(new { message = "Unsupported image type." });

            var wwwroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsDir = Path.Combine(wwwroot, "uploads");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsDir, fileName);

            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream, ct);
            }

            var path = $"/uploads/{fileName}";
            return Ok(new ImageUploadResult
            {
                Path = path,
                FileName = fileName,
                Size = file.Length,
                ContentType = file.ContentType
            });
        }
    }
}