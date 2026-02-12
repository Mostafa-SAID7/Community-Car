using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CommunityCar.Infrastructure.Interfaces;
using CommunityCar.Mvc.Controllers.Base;

namespace CommunityCar.Mvc.Controllers.Common;

[Authorize]
public class FileUploadController : BaseController
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FileUploadController> _logger;
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
    private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly string[] _allowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".txt" };

    public FileUploadController(
        IFileStorageService fileStorageService,
        ILogger<FileUploadController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile file, string? folder = null)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded" });
        }

        if (file.Length > _maxFileSize)
        {
            return Json(new { success = false, message = "File size exceeds 10MB limit" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedImageExtensions.Contains(extension))
        {
            return Json(new { success = false, message = "Invalid file type. Only images are allowed." });
        }

        try
        {
            var folderPath = string.IsNullOrEmpty(folder) ? "uploads/images" : $"uploads/{folder}";
            var fileUrl = await _fileStorageService.SaveFileAsync(file, folderPath);

            return Json(new { success = true, url = fileUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return Json(new { success = false, message = "Failed to upload image" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UploadPostImage(IFormFile file)
    {
        return await UploadImage(file, "posts/images");
    }

    [HttpPost]
    public async Task<IActionResult> UploadProfileImage(IFormFile file)
    {
        return await UploadImage(file, "profiles");
    }

    [HttpPost]
    public async Task<IActionResult> UploadGroupImage(IFormFile file)
    {
        return await UploadImage(file, "groups");
    }

    [HttpPost]
    public async Task<IActionResult> UploadEventImage(IFormFile file)
    {
        return await UploadImage(file, "events");
    }

    [HttpPost]
    public async Task<IActionResult> UploadDocument(IFormFile file, string? folder = null)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "No file uploaded" });
        }

        if (file.Length > _maxFileSize)
        {
            return Json(new { success = false, message = "File size exceeds 10MB limit" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedDocumentExtensions.Contains(extension))
        {
            return Json(new { success = false, message = "Invalid file type. Only documents are allowed." });
        }

        try
        {
            var folderPath = string.IsNullOrEmpty(folder) ? "uploads/documents" : $"uploads/{folder}";
            var fileUrl = await _fileStorageService.SaveFileAsync(file, folderPath);

            return Json(new { success = true, url = fileUrl, fileName = file.FileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return Json(new { success = false, message = "Failed to upload document" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFile(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl))
        {
            return Json(new { success = false, message = "File URL is required" });
        }

        try
        {
            await _fileStorageService.DeleteFileAsync(fileUrl);
            return Json(new { success = true, message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
            return Json(new { success = false, message = "Failed to delete file" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UploadMultiple(List<IFormFile> files, string? folder = null)
    {
        if (files == null || files.Count == 0)
        {
            return Json(new { success = false, message = "No files uploaded" });
        }

        var uploadedFiles = new List<object>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            if (file.Length > _maxFileSize)
            {
                errors.Add($"{file.FileName}: File size exceeds 10MB limit");
                continue;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedImageExtensions.Contains(extension))
            {
                errors.Add($"{file.FileName}: Invalid file type");
                continue;
            }

            try
            {
                var folderPath = string.IsNullOrEmpty(folder) ? "uploads/images" : $"uploads/{folder}";
                var fileUrl = await _fileStorageService.SaveFileAsync(file, folderPath);
                
                uploadedFiles.Add(new
                {
                    fileName = file.FileName,
                    url = fileUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                errors.Add($"{file.FileName}: Upload failed");
            }
        }

        return Json(new
        {
            success = uploadedFiles.Count > 0,
            files = uploadedFiles,
            errors = errors.Count > 0 ? errors : null
        });
    }

    [HttpGet]
    public async Task<IActionResult> CheckFileExists(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl))
        {
            return Json(new { exists = false });
        }

        try
        {
            var exists = await _fileStorageService.FileExistsAsync(fileUrl);
            return Json(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FileUrl}", fileUrl);
            return Json(new { exists = false });
        }
    }
}
