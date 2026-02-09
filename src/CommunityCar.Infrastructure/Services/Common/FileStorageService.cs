using CommunityCar.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Common;

/// <summary>
/// Service for handling file storage operations in wwwroot
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(
        IWebHostEnvironment environment,
        ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Save a file to the specified path
    /// </summary>
    public async Task<string> SaveFileAsync(IFormFile file, string folderPath, string? fileName = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null", nameof(file));
            }

            // Use provided filename or generate from original
            var finalFileName = fileName ?? $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // Ensure folder path doesn't start with slash
            folderPath = folderPath.TrimStart('/');

            // Create full directory path
            var fullFolderPath = Path.Combine(_environment.WebRootPath, folderPath);
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            // Full file path
            var filePath = Path.Combine(fullFolderPath, finalFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative URL (with forward slashes for web)
            var relativeUrl = $"/{folderPath}/{finalFileName}".Replace("\\", "/");
            
            _logger.LogInformation("File saved successfully: {FilePath}", relativeUrl);
            
            return relativeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file to {FolderPath}", folderPath);
            throw;
        }
    }

    /// <summary>
    /// Delete a file from storage
    /// </summary>
    public async Task DeleteFileAsync(string fileUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                return;
            }

            var physicalPath = GetPhysicalPath(fileUrl);

            if (File.Exists(physicalPath))
            {
                await Task.Run(() => File.Delete(physicalPath));
                _logger.LogInformation("File deleted successfully: {FilePath}", fileUrl);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", fileUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
            throw;
        }
    }

    /// <summary>
    /// Check if a file exists
    /// </summary>
    public async Task<bool> FileExistsAsync(string fileUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                return false;
            }

            var physicalPath = GetPhysicalPath(fileUrl);
            return await Task.Run(() => File.Exists(physicalPath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FileUrl}", fileUrl);
            return false;
        }
    }

    /// <summary>
    /// Get the physical path for a file URL
    /// </summary>
    public string GetPhysicalPath(string fileUrl)
    {
        // Remove leading slash and convert to system path separators
        var relativePath = fileUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
        return Path.Combine(_environment.WebRootPath, relativePath);
    }
}
