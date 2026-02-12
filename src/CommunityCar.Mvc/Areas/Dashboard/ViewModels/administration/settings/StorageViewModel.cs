namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels.Administration.Settings;

public class StorageViewModel
{
    public long TotalSize { get; set; }
    public long UploadsFolderSize { get; set; }
    public long ProfilePicturesSize { get; set; }
    public long PostImagesSize { get; set; }
    public long EventImagesSize { get; set; }
    public long MaxUploadSize { get; set; }
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();

    public string GetFormattedSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
