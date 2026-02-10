using System.Text.Json;
using System.Xml.Linq;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Entities.Dashboard.Localization;
using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Dashboard;

public class LocalizationService : ILocalizationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LocalizationService> _logger;
    private readonly string _resourcesPath;

    public LocalizationService(
        ApplicationDbContext context,
        ILogger<LocalizationService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _resourcesPath = Path.Combine(environment.ContentRootPath, "Resources", "Localization");
    }

    public async Task<PagedResult<LocalizationResourceDto>> GetResourcesAsync(LocalizationFilterDto filter)
    {
        var query = _context.Set<LocalizationResource>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Key))
            query = query.Where(r => r.Key.Contains(filter.Key));

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(r => r.Category == filter.Category);

        if (!string.IsNullOrWhiteSpace(filter.CultureCode))
            query = query.Where(r => r.CultureCode == filter.CultureCode);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(r => r.Key.Contains(filter.SearchTerm) || r.Value.Contains(filter.SearchTerm));

        if (filter.IsActive.HasValue)
            query = query.Where(r => r.IsActive == filter.IsActive.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(r => r.Category)
            .ThenBy(r => r.Key)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new LocalizationResourceDto
            {
                Id = r.Id,
                Key = r.Key,
                Category = r.Category,
                CultureCode = r.CultureCode,
                Value = r.Value,
                Description = r.Description,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                LastModifiedAt = r.LastModifiedAt,
                LastModifiedBy = r.LastModifiedBy
            })
            .ToListAsync();

        return new PagedResult<LocalizationResourceDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<LocalizationResourceDto?> GetResourceByIdAsync(Guid id)
    {
        var resource = await _context.Set<LocalizationResource>()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resource == null) return null;

        return new LocalizationResourceDto
        {
            Id = resource.Id,
            Key = resource.Key,
            Category = resource.Category,
            CultureCode = resource.CultureCode,
            Value = resource.Value,
            Description = resource.Description,
            IsActive = resource.IsActive,
            CreatedAt = resource.CreatedAt,
            LastModifiedAt = resource.LastModifiedAt,
            LastModifiedBy = resource.LastModifiedBy
        };
    }

    public async Task<LocalizationResourceDto?> GetResourceByKeyAsync(string key, string cultureCode)
    {
        var resource = await _context.Set<LocalizationResource>()
            .FirstOrDefaultAsync(r => r.Key == key && r.CultureCode == cultureCode);

        if (resource == null) return null;

        return new LocalizationResourceDto
        {
            Id = resource.Id,
            Key = resource.Key,
            Category = resource.Category,
            CultureCode = resource.CultureCode,
            Value = resource.Value,
            Description = resource.Description,
            IsActive = resource.IsActive,
            CreatedAt = resource.CreatedAt,
            LastModifiedAt = resource.LastModifiedAt,
            LastModifiedBy = resource.LastModifiedBy
        };
    }

    public async Task<LocalizationResourceDto> CreateResourceAsync(CreateLocalizationResourceDto dto)
    {
        var exists = await _context.Set<LocalizationResource>()
            .AnyAsync(r => r.Key == dto.Key && r.CultureCode == dto.CultureCode);

        if (exists)
            throw new InvalidOperationException($"Resource with key '{dto.Key}' already exists for culture '{dto.CultureCode}'");

        var resource = new LocalizationResource
        {
            Key = dto.Key,
            Category = dto.Category,
            CultureCode = dto.CultureCode,
            Value = dto.Value,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Set<LocalizationResource>().Add(resource);
        await _context.SaveChangesAsync();

        return new LocalizationResourceDto
        {
            Id = resource.Id,
            Key = resource.Key,
            Category = resource.Category,
            CultureCode = resource.CultureCode,
            Value = resource.Value,
            Description = resource.Description,
            IsActive = resource.IsActive,
            CreatedAt = resource.CreatedAt
        };
    }

    public async Task<LocalizationResourceDto> UpdateResourceAsync(UpdateLocalizationResourceDto dto)
    {
        var resource = await _context.Set<LocalizationResource>()
            .FirstOrDefaultAsync(r => r.Id == dto.Id);

        if (resource == null)
            throw new InvalidOperationException("Resource not found");

        resource.Value = dto.Value;
        resource.Description = dto.Description;
        resource.IsActive = dto.IsActive;
        resource.LastModifiedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        return new LocalizationResourceDto
        {
            Id = resource.Id,
            Key = resource.Key,
            Category = resource.Category,
            CultureCode = resource.CultureCode,
            Value = resource.Value,
            Description = resource.Description,
            IsActive = resource.IsActive,
            CreatedAt = resource.CreatedAt,
            LastModifiedAt = resource.LastModifiedAt,
            LastModifiedBy = resource.LastModifiedBy
        };
    }

    public async Task DeleteResourceAsync(Guid id)
    {
        var resource = await _context.Set<LocalizationResource>()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resource == null)
            throw new InvalidOperationException("Resource not found");

        _context.Set<LocalizationResource>().Remove(resource);
        await _context.SaveChangesAsync();
    }

    public async Task<List<string>> GetDistinctCategoriesAsync()
    {
        return await _context.Set<LocalizationResource>()
            .Select(r => r.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<List<string>> GetDistinctCulturesAsync()
    {
        return await _context.Set<LocalizationResource>()
            .Select(r => r.CultureCode)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<LocalizationStatisticsDto> GetStatisticsAsync()
    {
        var resources = await _context.Set<LocalizationResource>()
            .Where(r => r.IsActive)
            .ToListAsync();

        var distinctKeys = resources.Select(r => r.Key).Distinct().Count();
        var cultures = resources.Select(r => r.CultureCode).Distinct().ToList();

        var translationsByCulture = resources
            .GroupBy(r => r.CultureCode)
            .ToDictionary(g => g.Key, g => g.Count());

        var translationsByCategory = resources
            .GroupBy(r => r.Category)
            .ToDictionary(g => g.Key, g => g.Count());

        var expectedTranslations = distinctKeys * cultures.Count;
        var actualTranslations = resources.Count;
        var missingTranslations = expectedTranslations - actualTranslations;
        var completionPercentage = expectedTranslations > 0
            ? (double)actualTranslations / expectedTranslations * 100
            : 0;

        return new LocalizationStatisticsDto
        {
            TotalKeys = distinctKeys,
            TotalTranslations = actualTranslations,
            TranslationsByCulture = translationsByCulture,
            TranslationsByCategory = translationsByCategory,
            MissingTranslations = missingTranslations,
            CompletionPercentage = Math.Round(completionPercentage, 2)
        };
    }

    public async Task<Dictionary<string, string>> GetAllResourcesForCultureAsync(string cultureCode)
    {
        return await _context.Set<LocalizationResource>()
            .Where(r => r.CultureCode == cultureCode && r.IsActive)
            .ToDictionaryAsync(r => r.Key, r => r.Value);
    }

    public async Task<int> BulkImportAsync(BulkImportDto dto)
    {
        var imported = 0;

        foreach (var translation in dto.Translations)
        {
            var existing = await _context.Set<LocalizationResource>()
                .FirstOrDefaultAsync(r => r.Key == translation.Key && r.CultureCode == dto.CultureCode);

            if (existing != null)
            {
                if (dto.OverwriteExisting)
                {
                    existing.Value = translation.Value;
                    existing.LastModifiedAt = DateTimeOffset.UtcNow;
                    imported++;
                }
            }
            else
            {
                var resource = new LocalizationResource
                {
                    Key = translation.Key,
                    Category = "Imported",
                    CultureCode = dto.CultureCode,
                    Value = translation.Value,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                _context.Set<LocalizationResource>().Add(resource);
                imported++;
            }
        }

        await _context.SaveChangesAsync();
        return imported;
    }

    public async Task<Dictionary<string, Dictionary<string, string>>> ExportAllAsync()
    {
        var resources = await _context.Set<LocalizationResource>()
            .Where(r => r.IsActive)
            .ToListAsync();

        return resources
            .GroupBy(r => r.CultureCode)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(r => r.Key, r => r.Value)
            );
    }

    public async Task<List<LocalizationResourceDto>> GetMissingTranslationsAsync(string sourceCulture, string targetCulture)
    {
        var sourceKeys = await _context.Set<LocalizationResource>()
            .Where(r => r.CultureCode == sourceCulture)
            .Select(r => r.Key)
            .ToListAsync();

        var targetKeys = await _context.Set<LocalizationResource>()
            .Where(r => r.CultureCode == targetCulture)
            .Select(r => r.Key)
            .ToListAsync();

        var missingKeys = sourceKeys.Except(targetKeys).ToList();

        var missingResources = await _context.Set<LocalizationResource>()
            .Where(r => r.CultureCode == sourceCulture && missingKeys.Contains(r.Key))
            .Select(r => new LocalizationResourceDto
            {
                Id = r.Id,
                Key = r.Key,
                Category = r.Category,
                CultureCode = sourceCulture,
                Value = r.Value,
                Description = r.Description,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return missingResources;
    }

    public async Task SyncToJsonFilesAsync()
    {
        var cultures = await GetDistinctCulturesAsync();

        if (!Directory.Exists(_resourcesPath))
            Directory.CreateDirectory(_resourcesPath);

        foreach (var culture in cultures)
        {
            var resources = await GetAllResourcesForCultureAsync(culture);
            var filePath = Path.Combine(_resourcesPath, $"{culture}.json");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(resources, options);
            await File.WriteAllTextAsync(filePath, json);

            _logger.LogInformation("Synced {Count} resources to {FilePath}", resources.Count, filePath);
        }
    }

    public async Task<int> SyncFromJsonFilesAsync()
    {
        if (!Directory.Exists(_resourcesPath))
        {
            _logger.LogWarning("Resources path does not exist: {Path}", _resourcesPath);
            return 0;
        }

        var jsonFiles = Directory.GetFiles(_resourcesPath, "*.json");
        var totalImported = 0;

        foreach (var filePath in jsonFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var cultureCode = fileName;

            try
            {
                var jsonContent = await File.ReadAllTextAsync(filePath);
                var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

                if (translations == null || !translations.Any())
                    continue;

                foreach (var kvp in translations)
                {
                    var existing = await _context.Set<LocalizationResource>()
                        .FirstOrDefaultAsync(r => r.Key == kvp.Key && r.CultureCode == cultureCode);

                    if (existing == null)
                    {
                        var resource = new LocalizationResource
                        {
                            Key = kvp.Key,
                            Category = "General",
                            CultureCode = cultureCode,
                            Value = kvp.Value,
                            IsActive = true,
                            CreatedAt = DateTimeOffset.UtcNow
                        };

                        _context.Set<LocalizationResource>().Add(resource);
                        totalImported++;
                    }
                }

                _logger.LogInformation("Imported {Count} resources from {FilePath}", translations.Count, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing from file {FilePath}", filePath);
            }
        }

        if (totalImported > 0)
        {
            await _context.SaveChangesAsync();
        }

        return totalImported;
    }

    public async Task<List<string>> GetAvailableCulturesAsync()
    {
        return await GetDistinctCulturesAsync();
    }

    public async Task<List<string>> GetAvailableCategoriesAsync()
    {
        return await GetDistinctCategoriesAsync();
    }

    public async Task<Dictionary<string, string>> ExportResourcesAsync(string? culture, string? category)
    {
        var query = _context.Set<LocalizationResource>().AsQueryable();

        if (!string.IsNullOrEmpty(culture))
            query = query.Where(r => r.CultureCode == culture);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(r => r.Category == category);

        return await query.ToDictionaryAsync(r => r.Key, r => r.Value);
    }

    public async Task<int> SyncMissingKeysAsync(string sourceCulture, string targetCulture, bool overwriteExisting)
    {
        var missingKeys = await GetMissingTranslationsAsync(sourceCulture, targetCulture);
        var synced = 0;

        foreach (var missing in missingKeys)
        {
            var resource = new LocalizationResource
            {
                Key = missing.Key,
                Category = missing.Category,
                CultureCode = targetCulture,
                Value = missing.Value,
                Description = missing.Description,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Set<LocalizationResource>().Add(resource);
            synced++;
        }

        await _context.SaveChangesAsync();
        return synced;
    }

    public async Task RefreshCacheAsync()
    {
        await SyncToJsonFilesAsync();
    }

    public async Task<Dictionary<string, string>> GetCachedResourcesAsync(string cultureCode)
    {
        return await GetAllResourcesForCultureAsync(cultureCode);
    }

    public async Task<int> SyncFromResxFilesAsync()
    {
        var resourcesBasePath = Path.Combine(Path.GetDirectoryName(_resourcesPath)!);
        var totalImported = 0;

        if (!Directory.Exists(resourcesBasePath))
        {
            _logger.LogWarning("Resources base path does not exist: {Path}", resourcesBasePath);
            return 0;
        }

        var resxFiles = Directory.GetFiles(resourcesBasePath, "*.resx", SearchOption.AllDirectories);

        foreach (var filePath in resxFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var parts = fileName.Split('.');
                
                // Extract culture code (e.g., "ErrorController.en.resx" -> "en")
                string cultureCode = "en"; // default
                string category = "General";
                
                if (parts.Length >= 2)
                {
                    cultureCode = parts[^1]; // Last part is culture
                    category = string.Join(".", parts.Take(parts.Length - 1)); // Everything before culture
                }
                else
                {
                    category = parts[0];
                }

                // Determine category from path
                var relativePath = Path.GetRelativePath(resourcesBasePath, Path.GetDirectoryName(filePath)!);
                if (!string.IsNullOrEmpty(relativePath) && relativePath != ".")
                {
                    category = $"{relativePath.Replace(Path.DirectorySeparatorChar, '.')}.{category}";
                }

                var translations = ParseResxFile(filePath);

                foreach (var kvp in translations)
                {
                    var key = $"{category}.{kvp.Key}";
                    
                    var existing = await _context.Set<LocalizationResource>()
                        .FirstOrDefaultAsync(r => r.Key == key && r.CultureCode == cultureCode);

                    if (existing == null)
                    {
                        var resource = new LocalizationResource
                        {
                            Key = key,
                            Category = category,
                            CultureCode = cultureCode,
                            Value = kvp.Value,
                            IsActive = true,
                            CreatedAt = DateTimeOffset.UtcNow
                        };

                        _context.Set<LocalizationResource>().Add(resource);
                        totalImported++;
                    }
                }

                _logger.LogInformation("Imported {Count} resources from {FilePath}", translations.Count, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing from RESX file {FilePath}", filePath);
            }
        }

        if (totalImported > 0)
        {
            await _context.SaveChangesAsync();
        }

        return totalImported;
    }

    public async Task<int> SyncToResxFilesAsync()
    {
        var resourcesBasePath = Path.Combine(Path.GetDirectoryName(_resourcesPath)!);
        var totalExported = 0;

        if (!Directory.Exists(resourcesBasePath))
        {
            Directory.CreateDirectory(resourcesBasePath);
        }

        var resources = await _context.Set<LocalizationResource>()
            .Where(r => r.IsActive)
            .ToListAsync();

        // Group by category and culture
        var grouped = resources
            .GroupBy(r => new { r.Category, r.CultureCode })
            .ToList();

        foreach (var group in grouped)
        {
            try
            {
                var category = group.Key.Category;
                var culture = group.Key.CultureCode;

                // Determine file path based on category
                var categoryParts = category.Split('.');
                var folderPath = resourcesBasePath;
                var fileName = category;

                // If category contains path separators, create folder structure
                if (categoryParts.Length > 1)
                {
                    folderPath = Path.Combine(resourcesBasePath, string.Join(Path.DirectorySeparatorChar, categoryParts.Take(categoryParts.Length - 1)));
                    fileName = categoryParts[^1];
                }

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, $"{fileName}.{culture}.resx");

                // Create RESX content
                var translations = group.ToDictionary(
                    r => r.Key.Replace($"{category}.", ""), // Remove category prefix from key
                    r => r.Value
                );

                CreateResxFile(filePath, translations);
                totalExported += translations.Count;

                _logger.LogInformation("Exported {Count} resources to {FilePath}", translations.Count, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to RESX for category {Category} culture {Culture}", 
                    group.Key.Category, group.Key.CultureCode);
            }
        }

        return totalExported;
    }

    private Dictionary<string, string> ParseResxFile(string filePath)
    {
        var result = new Dictionary<string, string>();

        try
        {
            var doc = XDocument.Load(filePath);
            var dataElements = doc.Descendants("data");

            foreach (var element in dataElements)
            {
                var name = element.Attribute("name")?.Value;
                var value = element.Element("value")?.Value;

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    result[name] = value;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing RESX file {FilePath}", filePath);
        }

        return result;
    }

    private void CreateResxFile(string filePath, Dictionary<string, string> translations)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("root",
                // Schema definition
                new XElement(XName.Get("schema", "http://www.w3.org/2001/XMLSchema") + "xsd",
                    new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                    new XAttribute(XNamespace.Xmlns + "msdata", "urn:schemas-microsoft-com:xml-msdata"),
                    new XAttribute("id", "root")
                ),
                // Resource headers
                new XElement("resheader",
                    new XAttribute("name", "resmimetype"),
                    new XElement("value", "text/microsoft-resx")),
                new XElement("resheader",
                    new XAttribute("name", "version"),
                    new XElement("value", "2.0")),
                new XElement("resheader",
                    new XAttribute("name", "reader"),
                    new XElement("value", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")),
                new XElement("resheader",
                    new XAttribute("name", "writer"),
                    new XElement("value", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")),
                // Data elements
                translations.Select(kvp =>
                    new XElement("data",
                        new XAttribute("name", kvp.Key),
                        new XAttribute(XNamespace.Xml + "space", "preserve"),
                        new XElement("value", kvp.Value)
                    )
                )
            )
        );

        doc.Save(filePath);
    }

}
