using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityCar.Infrastructure.Interfaces;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels.Administration.Settings;

namespace CommunityCar.Mvc.Areas.Dashboard.Controllers.Administration.Settings;

[Area("Dashboard")]
[Authorize(Roles = "Admin")]
[Route("{culture}/Dashboard/Administration/Settings/[controller]")]
public class StorageController : Controller
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<StorageController> _logger;

    public StorageController(
        IFileStorageService fileStorageService,
        IWebHostEnvironment environment,
        ILogger<StorageController> logger)
    {
        _fileStorageService = fileStorageService;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var viewModel = new StorageViewModel
        {
            TotalSize = await GetTotalStorageSizeAsync(),
            UploadsFolderSize = await GetFolderSizeAsync("uploads"),
            ProfilePicturesSize = await GetFolderSizeAsync("uploads/profiles"),
            PostImagesSize = await GetFolderSizeAsync("uploads/posts/images"),
            EventImagesSize = await GetFolderSizeAsync("uploads/events"),
            MaxUploadSize = GetMaxUploadSize(),
            AllowedExtensions = GetAllowedExtensions()
        };

        return View(viewModel);
    }

    [HttpPost("ClearCache")]
    [ValidateAntiForgeryToken]
    public IActionResult ClearCache()
    {
        try
        {
            // Clear temporary files
            var tempPath = Path.Combine(_environment.WebRootPath, "temp");
            if (Directory.Exists(tempPath))
            {
                var files = Directory.GetFiles(tempPath);
                foreach (var file in files)
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete temp file: {File}", file);
                    }
                }
            }

            TempData["Success"] = "Cache cleared successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            TempData["Error"] = "Failed to clear cache";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<long> GetTotalStorageSizeAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                    return 0;

                return GetDirectorySize(uploadsPath);
            }
            catch
            {
                return 0;
            }
        });
    }

    private async Task<long> GetFolderSizeAsync(string relativePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, relativePath);
                if (!Directory.Exists(fullPath))
                    return 0;

                return GetDirectorySize(fullPath);
            }
            catch
            {
                return 0;
            }
        });
    }

    private long GetDirectorySize(string path)
    {
        var directoryInfo = new DirectoryInfo(path);
        return directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
    }

    private long GetMaxUploadSize()
    {
        // Default ASP.NET Core max request body size is 30MB
        return 30 * 1024 * 1024;
    }

    private string[] GetAllowedExtensions()
    {
        return new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".csv" };
    }
}
