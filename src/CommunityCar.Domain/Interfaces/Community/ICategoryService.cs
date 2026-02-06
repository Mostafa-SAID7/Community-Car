using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Domain.Interfaces.Community;

public interface ICategoryService
{
    Task<Result<IEnumerable<CategoryDto>>> GetAllCategoriesAsync(bool includeInactive = false);
    Task<Result<CategoryDto>> GetCategoryByIdAsync(Guid id);
    Task<Result<CategoryDto>> GetCategoryBySlugAsync(string slug);
    Task<Result<CategoryDto>> CreateCategoryAsync(string name, string slug, string? description = null, string? icon = null, string? color = null, int displayOrder = 0);
    Task<Result<CategoryDto>> UpdateCategoryAsync(Guid id, string name, string slug, string? description = null, string? icon = null, string? color = null, int? displayOrder = null);
    Task<Result> DeleteCategoryAsync(Guid id);
    Task<Result> ActivateCategoryAsync(Guid id);
    Task<Result> DeactivateCategoryAsync(Guid id);
}
