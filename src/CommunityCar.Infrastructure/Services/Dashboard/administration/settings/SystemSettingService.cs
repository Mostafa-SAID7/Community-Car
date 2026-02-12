using AutoMapper;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard.Administration.Settings;
using CommunityCar.Domain.Entities.Dashboard.settings;
using CommunityCar.Domain.Enums.Dashboard.settings;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Dashboard.Administration.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CommunityCar.Infrastructure.Services.Dashboard.Administration.Settings;

public class SystemSettingService : ISystemSettingService
{
    private readonly IRepository<SystemSetting> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SystemSettingService> _logger;

    public SystemSettingService(
        IRepository<SystemSetting> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SystemSettingService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<SystemSettingDto>> GetSettingsAsync(SystemSettingFilterDto filter)
    {
        var query = _repository.GetQueryable();

        // Apply filters
        if (filter.Category.HasValue)
            query = query.Where(s => s.Category == filter.Category.Value);

        if (filter.DataType.HasValue)
            query = query.Where(s => s.DataType == filter.DataType.Value);

        if (filter.IsReadOnly.HasValue)
            query = query.Where(s => s.IsReadOnly == filter.IsReadOnly.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(s =>
                s.Key.ToLower().Contains(searchTerm) ||
                (s.Description != null && s.Description.ToLower().Contains(searchTerm)));
        }

        // Apply sorting
        query = filter.SortBy?.ToLower() switch
        {
            "key" => filter.SortDescending ? query.OrderByDescending(s => s.Key) : query.OrderBy(s => s.Key),
            "category" => filter.SortDescending ? query.OrderByDescending(s => s.Category) : query.OrderBy(s => s.Category),
            "updatedat" => filter.SortDescending ? query.OrderByDescending(s => s.ModifiedAt ?? s.CreatedAt) : query.OrderBy(s => s.ModifiedAt ?? s.CreatedAt),
            _ => filter.SortDescending ? query.OrderByDescending(s => s.DisplayOrder) : query.OrderBy(s => s.DisplayOrder)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = items.Select(s => MapToDto(s)).ToList();

        return new PagedResult<SystemSettingDto>(
            dtos,
            totalCount,
            filter.Page,
            filter.PageSize);
    }

    public async Task<SystemSettingDto?> GetSettingByIdAsync(Guid id)
    {
        var setting = await _repository.GetByIdAsync(id);
        return setting != null ? MapToDto(setting) : null;
    }

    public async Task<SystemSettingDto?> GetSettingByKeyAsync(string key)
    {
        var setting = await _repository.GetQueryable()
            .FirstOrDefaultAsync(s => s.Key == key);
        return setting != null ? MapToDto(setting) : null;
    }

    public async Task<string?> GetSettingValueAsync(string key)
    {
        var setting = await _repository.GetQueryable()
            .FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task<T?> GetSettingValueAsync<T>(string key)
    {
        var value = await GetSettingValueAsync(key);
        if (string.IsNullOrEmpty(value))
            return default;

        try
        {
            if (typeof(T) == typeof(string))
                return (T)(object)value;

            if (typeof(T) == typeof(int))
                return (T)(object)int.Parse(value);

            if (typeof(T) == typeof(bool))
                return (T)(object)bool.Parse(value);

            if (typeof(T) == typeof(decimal))
                return (T)(object)decimal.Parse(value);

            if (typeof(T) == typeof(DateTime))
                return (T)(object)DateTime.Parse(value);

            // Try JSON deserialization for complex types
            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing setting value for key {Key}", key);
            return default;
        }
    }

    public async Task<SystemSettingDto> CreateSettingAsync(CreateSystemSettingDto dto)
    {
        // Check if key already exists
        var exists = await SettingExistsAsync(dto.Key);
        if (exists)
            throw new InvalidOperationException($"Setting with key '{dto.Key}' already exists");

        var setting = new SystemSetting(
            dto.Key,
            dto.Value,
            dto.Category,
            dto.DataType,
            dto.Description,
            dto.IsEncrypted,
            dto.IsReadOnly,
            dto.DefaultValue,
            dto.ValidationRegex,
            dto.DisplayOrder);

        await _repository.AddAsync(setting);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created system setting: {Key}", dto.Key);

        return MapToDto(setting);
    }

    public async Task UpdateSettingAsync(UpdateSystemSettingDto dto)
    {
        var setting = await _repository.GetByIdAsync(dto.Id);
        if (setting == null)
            throw new KeyNotFoundException($"Setting with ID {dto.Id} not found");

        setting.UpdateValue(dto.Value);
        setting.UpdateDescription(dto.Description);
        setting.UpdateDisplayOrder(dto.DisplayOrder);

        _repository.Update(setting);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated system setting: {Key}", setting.Key);
    }

    public async Task UpdateSettingValueAsync(string key, string value)
    {
        var setting = await _repository.GetQueryable()
            .FirstOrDefaultAsync(s => s.Key == key);

        if (setting == null)
            throw new KeyNotFoundException($"Setting with key '{key}' not found");

        setting.UpdateValue(value);
        _repository.Update(setting);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated setting value for key: {Key}", key);
    }

    public async Task DeleteSettingAsync(Guid id)
    {
        var setting = await _repository.GetByIdAsync(id);
        if (setting == null)
            throw new KeyNotFoundException($"Setting with ID {id} not found");

        if (setting.IsReadOnly)
            throw new InvalidOperationException("Cannot delete read-only setting");

        _repository.Delete(setting);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted system setting: {Key}", setting.Key);
    }

    public async Task ResetToDefaultAsync(Guid id)
    {
        var setting = await _repository.GetByIdAsync(id);
        if (setting == null)
            throw new KeyNotFoundException($"Setting with ID {id} not found");

        setting.ResetToDefault();
        _repository.Update(setting);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Reset setting to default: {Key}", setting.Key);
    }

    public async Task<SettingsSummaryDto> GetSummaryAsync()
    {
        var settings = await _repository.GetQueryable().ToListAsync();

        return new SettingsSummaryDto
        {
            TotalSettings = settings.Count,
            ReadOnlySettings = settings.Count(s => s.IsReadOnly),
            EncryptedSettings = settings.Count(s => s.IsEncrypted),
            SettingsByCategory = settings
                .GroupBy(s => s.Category.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            LastModified = settings.Any() ? settings.Max(s => s.ModifiedAt ?? s.CreatedAt) : DateTimeOffset.UtcNow
        };
    }

    public async Task<Dictionary<string, SystemSettingDto>> GetSettingsByCategoryAsync(SettingCategory category)
    {
        var settings = await _repository.GetQueryable()
            .Where(s => s.Category == category)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

        return settings.ToDictionary(s => s.Key, s => MapToDto(s));
    }

    public async Task<bool> SettingExistsAsync(string key)
    {
        return await _repository.GetQueryable()
            .AnyAsync(s => s.Key == key);
    }

    public async Task BulkUpdateSettingsAsync(Dictionary<string, string> settings)
    {
        foreach (var kvp in settings)
        {
            var setting = await _repository.GetQueryable()
                .FirstOrDefaultAsync(s => s.Key == kvp.Key);

            if (setting != null && !setting.IsReadOnly)
            {
                setting.UpdateValue(kvp.Value);
                _repository.Update(setting);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Bulk updated {Count} settings", settings.Count);
    }

    private SystemSettingDto MapToDto(SystemSetting setting)
    {
        return new SystemSettingDto
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.IsEncrypted ? "********" : setting.Value,
            Description = setting.Description,
            Category = setting.Category,
            CategoryText = setting.Category.ToString(),
            DataType = setting.DataType,
            DataTypeText = setting.DataType.ToString(),
            IsEncrypted = setting.IsEncrypted,
            IsReadOnly = setting.IsReadOnly,
            DefaultValue = setting.DefaultValue,
            ValidationRegex = setting.ValidationRegex,
            DisplayOrder = setting.DisplayOrder,
            CreatedAt = setting.CreatedAt,
            UpdatedAt = setting.ModifiedAt ?? setting.CreatedAt
        };
    }
}
