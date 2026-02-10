# Quick Fix Guide - Refactoring Services to Use IUnitOfWork

## Current Status
- ✅ ReviewService - COMPLETED
- 🔄 GuideService - PARTIALLY DONE (needs completion)
- ⏳ 15 other services - PENDING

## Immediate Fix Needed

### GuideService - Replace All `_context` References

The GuideService was partially refactored. Here's what needs to be done:

#### Global Find & Replace Operations

1. **Replace DbContext queries with Repository queries:**
   ```
   FIND: _context.Set<Guide>()
   REPLACE: _uow.Repository<Guide>().GetQueryable()
   ```

2. **Replace SaveChangesAsync:**
   ```
   FIND: await _context.SaveChangesAsync();
   REPLACE: await _uow.SaveChangesAsync();
   ```

3. **Replace Add operations:**
   ```
   FIND: _context.Set<GuideStep>().Add(
   REPLACE: await _uow.Repository<GuideStep>().AddAsync(
   ```

4. **Replace Remove operations:**
   ```
   FIND: _context.Set<Guide>().Remove(
   REPLACE: _uow.Repository<Guide>().Delete(
   ```

5. **For other entity types in GuideService:**
   - `GuideReaction` - Replace `_context.Set<GuideReaction>()` with `_uow.Repository<GuideReaction>()`
   - `GuideBookmark` - Replace `_context.Set<GuideBookmark>()` with `_uow.Repository<GuideBookmark>()`
   - `GuideStep` - Replace `_context.Set<GuideStep>()` with `_uow.Repository<GuideStep>()`
   - `GuideComment` - Replace `_context.Set<GuideComment>()` with `_uow.Repository<GuideComment>()`

#### Specific Patterns to Fix

**Pattern 1: FirstOrDefaultAsync**
```csharp
// OLD
var guide = await _context.Set<Guide>()
    .FirstOrDefaultAsync(g => g.Id == guideId);

// NEW
var guide = await _uow.Repository<Guide>()
    .FirstOrDefaultAsync(g => g.Id == guideId);
```

**Pattern 2: Queries with Include**
```csharp
// OLD
var guide = await _context.Set<Guide>()
    .Include(g => g.Author)
    .FirstOrDefaultAsync(g => g.Id == guideId);

// NEW
var query = _uow.Repository<Guide>().GetQueryable();
var guide = await query
    .Include(g => g.Author)
    .FirstOrDefaultAsync(g => g.Id == guideId);
```

**Pattern 3: AnyAsync**
```csharp
// OLD
while (await _context.Set<Guide>().AnyAsync(g => g.Slug == slug))

// NEW
while (await _uow.Repository<Guide>().CountAsync(g => g.Slug == slug) > 0)
```

**Pattern 4: Where queries**
```csharp
// OLD
var steps = await _context.Set<GuideStep>()
    .Where(s => s.GuideId == guideId)
    .OrderBy(s => s.StepNumber)
    .ToListAsync();

// NEW
var query = _uow.Repository<GuideStep>().GetQueryable();
var steps = await query
    .Where(s => s.GuideId == guideId)
    .OrderBy(s => s.StepNumber)
    .ToListAsync();
```

**Pattern 5: GroupBy and ToDictionary**
```csharp
// OLD
return await _context.Set<Guide>()
    .Where(g => g.Status == GuideStatus.Published)
    .GroupBy(g => g.Category)
    .Select(g => new { Category = g.Key, Count = g.Count() })
    .ToDictionaryAsync(x => x.Category, x => x.Count);

// NEW
var query = _uow.Repository<Guide>().GetQueryable();
return await query
    .Where(g => g.Status == GuideStatus.Published)
    .GroupBy(g => g.Category)
    .Select(g => new { Category = g.Key, Count = g.Count() })
    .ToDictionaryAsync(x => x.Category, x => x.Count);
```

---

## Step-by-Step Fix for GuideService

### Step 1: Add Missing Using Statement
At the top of GuideService.cs, ensure you have:
```csharp
using Microsoft.EntityFrameworkCore; // For Include(), ToListAsync(), etc.
```

### Step 2: Fix All Methods

I'll provide the corrected code for each method that has errors:

#### UpdateGuideAsync
```csharp
public async Task<Guide> UpdateGuideAsync(
    Guid guideId,
    string title,
    string content,
    string summary,
    string category,
    GuideDifficulty difficulty,
    int estimatedTimeMinutes)
{
    var guide = await _uow.Repository<Guide>()
        .FirstOrDefaultAsync(g => g.Id == guideId);

    if (guide == null)
        throw new NotFoundException("Guide not found");

    var oldSlug = guide.Slug;
    guide.Update(title, content, summary, category, difficulty, estimatedTimeMinutes);
    
    if (guide.Slug != oldSlug)
    {
        var baseSlug = guide.Slug;
        var slug = baseSlug;
        var counter = 1;
        
        while (await _uow.Repository<Guide>()
            .CountAsync(g => g.Slug == slug && g.Id != guideId) > 0)
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }
        
        if (slug != baseSlug)
        {
            guide.GetType().GetProperty("Slug")!.SetValue(guide, slug);
        }
    }
    
    _uow.Repository<Guide>().Update(guide);
    await _uow.SaveChangesAsync();

    _logger.LogInformation("Guide updated: {GuideId}", guideId);
    return guide;
}
```

#### DeleteGuideAsync
```csharp
public async Task DeleteGuideAsync(Guid guideId)
{
    var guide = await _uow.Repository<Guide>()
        .FirstOrDefaultAsync(g => g.Id == guideId);

    if (guide == null)
        throw new NotFoundException("Guide not found");

    _uow.Repository<Guide>().Delete(guide);
    await _uow.SaveChangesAsync();

    _logger.LogInformation("Guide deleted: {GuideId}", guideId);
}
```

---

## Automated Fix Script

You can use this PowerShell script to automatically fix most issues:

```powershell
$file = "src/CommunityCar.Infrastructure/Services/Community/GuideService.cs"
$content = Get-Content $file -Raw

# Replace all _context references
$content = $content -replace '_context\.Set<Guide>\(\)', '_uow.Repository<Guide>().GetQueryable()'
$content = $content -replace '_context\.Set<GuideStep>\(\)', '_uow.Repository<GuideStep>().GetQueryable()'
$content = $content -replace '_context\.Set<GuideComment>\(\)', '_uow.Repository<GuideComment>().GetQueryable()'
$content = $content -replace '_context\.Set<GuideReaction>\(\)', '_uow.Repository<GuideReaction>().GetQueryable()'
$content = $content -replace '_context\.Set<GuideBookmark>\(\)', '_uow.Repository<GuideBookmark>().GetQueryable()'
$content = $content -replace 'await _context\.SaveChangesAsync\(\);', 'await _uow.SaveChangesAsync();'

Set-Content $file $content
```

---

## Priority Order for Remaining Services

After fixing GuideService, refactor in this order:

### High Priority (Do First)
1. **PostService** - Most used service
2. **QuestionService** - Core feature
3. **FriendshipService** - Social features
4. **SecurityService** - Security critical

### Medium Priority
5. NewsService
6. EventService
7. MapService
8. AuditLogService
9. SecurityAlertService

### Low Priority
10. HealthService
11. KPIService
12. LocalizationService
13. SettingsService
14. ContentActivityService
15. UserActivityService

---

## Testing After Each Refactoring

After refactoring each service:

1. **Build the project:**
   ```bash
   dotnet build
   ```

2. **Run tests (if available):**
   ```bash
   dotnet test
   ```

3. **Manual testing:**
   - Test CRUD operations
   - Test queries
   - Test transactions

---

## Common Mistakes to Avoid

1. ❌ **Don't forget to add `await` for AddAsync:**
   ```csharp
   // WRONG
   _uow.Repository<Entity>().AddAsync(entity);
   
   // CORRECT
   await _uow.Repository<Entity>().AddAsync(entity);
   ```

2. ❌ **Don't forget GetQueryable() for complex queries:**
   ```csharp
   // WRONG
   var items = await _uow.Repository<Entity>()
       .Include(e => e.Related)
       .ToListAsync();
   
   // CORRECT
   var query = _uow.Repository<Entity>().GetQueryable();
   var items = await query
       .Include(e => e.Related)
       .ToListAsync();
   ```

3. ❌ **Don't use AnyAsync directly on Repository:**
   ```csharp
   // WRONG
   if (await _uow.Repository<Entity>().AnyAsync(e => e.Id == id))
   
   // CORRECT
   if (await _uow.Repository<Entity>().CountAsync(e => e.Id == id) > 0)
   ```

---

## Next Steps

1. ✅ Fix GuideService completely (all 58 errors)
2. Build and test
3. Move to next service (PostService recommended)
4. Repeat until all 16 services are refactored

---

**Last Updated**: 2026-02-10
**Status**: GuideService needs completion - 58 errors to fix
