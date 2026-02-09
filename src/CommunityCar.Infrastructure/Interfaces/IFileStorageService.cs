using Microsoft.AspNetCore.Http;

namespace CommunityCar.Infrastructure.Interfaces;

/// <summary>
/// Service for handling file storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Save a file to the specified path
    /// </summary>
    /// <param name="file">The file to save</param>
    /// <param name="folderPath">The folder path relative to wwwroot (e.g., "uploads/posts/images")</param>
    /// <param name="fileName">Optional custom filename. If not provided, uses original filename</param>
    /// <returns>The relative URL to the saved file</returns>
    Task<string> SaveFileAsync(IFormFile file, string folderPath, string? fileName = null);

    /// <summary>
    /// Delete a file from storage
    /// </summary>
    /// <param name="fileUrl">The relative URL of the file to delete</param>
    Task DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Check if a file exists
    /// </summary>
    /// <param name="fileUrl">The relative URL of the file</param>
    /// <returns>True if file exists, false otherwise</returns>
    Task<bool> FileExistsAsync(string fileUrl);

    /// <summary>
    /// Get the physical path for a file URL
    /// </summary>
    /// <param name="fileUrl">The relative URL of the file</param>
    /// <returns>The physical file path</returns>
    string GetPhysicalPath(string fileUrl);
}
