using AutoMapper;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.Common;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        IRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<CategoryDto>>> GetAllCategoriesAsync(bool includeInactive = false)
    {
        try
        {
            IQueryable<Category> query = _categoryRepository.GetQueryable()
                .Include(c => c.Questions);

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            var categories = await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return Result.Success<IEnumerable<CategoryDto>>(categoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            return Result.Failure<IEnumerable<CategoryDto>>("Failed to retrieve categories");
        }
    }

    public async Task<Result<CategoryDto>> GetCategoryByIdAsync(Guid id)
    {
        try
        {
            var category = await _categoryRepository.GetQueryable()
                .Include(c => c.Questions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return Result.Failure<CategoryDto>("Category not found");
            }

            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Result.Success<CategoryDto>(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by id {CategoryId}", id);
            return Result.Failure<CategoryDto>("Failed to retrieve category");
        }
    }

    public async Task<Result<CategoryDto>> GetCategoryBySlugAsync(string slug)
    {
        try
        {
            var category = await _categoryRepository.GetQueryable()
                .Include(c => c.Questions)
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (category == null)
            {
                return Result.Failure<CategoryDto>("Category not found");
            }

            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Result.Success<CategoryDto>(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by slug {Slug}", slug);
            return Result.Failure<CategoryDto>("Failed to retrieve category");
        }
    }

    public async Task<Result<CategoryDto>> CreateCategoryAsync(string name, string slug, string? description = null, string? icon = null, string? color = null, int displayOrder = 0)
    {
        try
        {
            // Check if slug already exists
            var existingCategory = await _categoryRepository.GetQueryable()
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (existingCategory != null)
            {
                return Result.Failure<CategoryDto>("A category with this slug already exists");
            }

            var category = new Category(name, slug, description, icon, color, displayOrder);
            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Result.Success<CategoryDto>(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return Result.Failure<CategoryDto>("Failed to create category");
        }
    }

    public async Task<Result<CategoryDto>> UpdateCategoryAsync(Guid id, string name, string slug, string? description = null, string? icon = null, string? color = null, int? displayOrder = null)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return Result.Failure<CategoryDto>("Category not found");
            }

            // Check if slug is taken by another category
            var existingCategory = await _categoryRepository.GetQueryable()
                .FirstOrDefaultAsync(c => c.Slug == slug && c.Id != id);

            if (existingCategory != null)
            {
                return Result.Failure<CategoryDto>("A category with this slug already exists");
            }

            category.Update(name, slug, description, icon, color, displayOrder);
            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();

            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Result.Success<CategoryDto>(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return Result.Failure<CategoryDto>("Failed to update category");
        }
    }

    public async Task<Result> DeleteCategoryAsync(Guid id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return Result.Failure("Category not found");
            }

            _categoryRepository.Delete(category);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return Result.Failure("Failed to delete category");
        }
    }

    public async Task<Result> ActivateCategoryAsync(Guid id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return Result.Failure("Category not found");
            }

            category.Activate();
            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating category {CategoryId}", id);
            return Result.Failure("Failed to activate category");
        }
    }

    public async Task<Result> DeactivateCategoryAsync(Guid id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return Result.Failure("Category not found");
            }

            category.Deactivate();
            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating category {CategoryId}", id);
            return Result.Failure("Failed to deactivate category");
        }
    }
}

