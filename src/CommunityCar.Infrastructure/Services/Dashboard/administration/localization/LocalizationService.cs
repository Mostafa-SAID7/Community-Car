using System.Text.Json;
using System.Xml.Linq;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard.Administration.Localization;
using CommunityCar.Domain.Entities.Dashboard.Localization;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Dashboard.Administration.Localization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Dashboard.Administration.Localization;

public class LocalizationService : ILocalizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LocalizationService> _logger;
    private readonly string _resourcesPath;
    private IRepository<LocalizationResource> Repository => _unitOfWork.Repository<LocalizationResource>();

    public LocalizationService(
        IUnitOfWork unitOfWork,
        ILogger<LocalizationService> logger,
        IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _resourcesPath = Path.Combine(environment.ContentRootPath, "Resources", "Localization");
    }

    public async Task<PagedResult<LocalizationResourceDto>> GetResourcesAsync(LocalizationFilterDto filter)
    {
        var query = Repository.GetQueryable();

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
        var resource = await Repository.GetByIdAsync(id);

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
        var resource = await Repository.FirstOrDefaultAsync(r => r.Key == key && r.CultureCode == cultureCode);

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
        var exists = await Repository.CountAsync(r => r.Key == dto.Key && r.CultureCode == dto.CultureCode) > 0;

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

        await Repository.AddAsync(resource);
        await _unitOfWork.SaveChangesAsync();

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
        var resource = await Repository.GetByIdAsync(dto.Id);

        if (resource == null)
            throw new InvalidOperationException("Resource not found");

        resource.Value = dto.Value;
        resource.Description = dto.Description;
        resource.IsActive = dto.IsActive;
        resource.LastModifiedAt = DateTimeOffset.UtcNow;

        Repository.Update(resource);
        await _unitOfWork.SaveChangesAsync();

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
        var resource = await Repository.GetByIdAsync(id);

        if (resource == null)
            throw new InvalidOperationException("Resource not found");

        Repository.Delete(resource);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<string>> GetDistinctCategoriesAsync()
    {
        return await Repository.GetQueryable()
            .Select(r => r.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<List<string>> GetDistinctCulturesAsync()
    {
        return await Repository.GetQueryable()
            .Select(r => r.CultureCode)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<LocalizationStatisticsDto> GetStatisticsAsync()
    {
        var resources = await Repository.WhereAsync(r => r.IsActive);
        var resourcesList = resources.ToList();

        var distinctKeys = resourcesList.Select(r => r.Key).Distinct().Count();
        var cultures = resourcesList.Select(r => r.CultureCode).Distinct().ToList();

        var translationsByCulture = resourcesList
            .GroupBy(r => r.CultureCode)
            .ToDictionary(g => g.Key, g => g.Count());

        var translationsByCategory = resourcesList
            .GroupBy(r => r.Category)
            .ToDictionary(g => g.Key, g => g.Count());

        var expectedTranslations = distinctKeys * cultures.Count;
        var actualTranslations = resourcesList.Count;
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
        var resources = await Repository.WhereAsync(r => r.CultureCode == cultureCode && r.IsActive);
        return resources.ToDictionary(r => r.Key, r => r.Value);
    }

    public async Task<int> BulkImportAsync(BulkImportDto dto)
    {
        var imported = 0;

        foreach (var translation in dto.Translations)
        {
            var existing = await Repository.FirstOrDefaultAsync(r => r.Key == translation.Key && r.CultureCode == dto.CultureCode);

            if (existing != null)
            {
                if (dto.OverwriteExisting)
                {
                    existing.Value = translation.Value;
                    existing.LastModifiedAt = DateTimeOffset.UtcNow;
                    Repository.Update(existing);
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

                await Repository.AddAsync(resource);
                imported++;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return imported;
    }

    public async Task<Dictionary<string, Dictionary<string, string>>> ExportAllAsync()
    {
        var resources = await Repository.WhereAsync(r => r.IsActive);
        var resourcesList = resources.ToList();

        return resourcesList
            .GroupBy(r => r.CultureCode)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(r => r.Key, r => r.Value)
            );
    }

    public async Task<List<LocalizationResourceDto>> GetMissingTranslationsAsync(string sourceCulture, string targetCulture)
    {
        var sourceKeys = await Repository.GetQueryable()
            .Where(r => r.CultureCode == sourceCulture)
            .Select(r => r.Key)
            .ToListAsync();

        var targetKeys = await Repository.GetQueryable()
            .Where(r => r.CultureCode == targetCulture)
            .Select(r => r.Key)
            .ToListAsync();

        var missingKeys = sourceKeys.Except(targetKeys).ToList();

        var missingResources = await Repository.GetQueryable()
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
        var query = Repository.GetQueryable();

        if (!string.IsNullOrEmpty(culture))
            query = query.Where(r => r.CultureCode == culture);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(r => r.Category == category);

        var resources = await query.ToListAsync();
        return resources.ToDictionary(r => r.Key, r => r.Value);
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

            await Repository.AddAsync(resource);
            synced++;
        }

        await _unitOfWork.SaveChangesAsync();
        return synced;
    }

    public async Task RefreshCacheAsync()
    {
        // Cache refresh logic - currently no caching implemented
        // This method is kept for future caching implementation
        await Task.CompletedTask;
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
                    
                    var existing = await Repository.FirstOrDefaultAsync(r => r.Key == key && r.CultureCode == cultureCode);

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

                        await Repository.AddAsync(resource);
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
            await _unitOfWork.SaveChangesAsync();
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

        var resources = await Repository.WhereAsync(r => r.IsActive);
        var resourcesList = resources.ToList();

        // Group by category and culture
        var grouped = resourcesList
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

