# ⚠️ IMMEDIATE ACTION REQUIRED - Build Errors

## 🔴 Current Status: BUILD FAILING

**Error Count**: 58 errors in GuideService.cs
**Root Cause**: Partial refactoring - GuideService has mixed `_context` and `_uow` usage

---

## 🎯 What Happened

We started refactoring services from `ApplicationDbContext` to `IUnitOfWork` pattern:
- ✅ **ReviewService**: Fully refactored and working
- ⚠️ **GuideService**: Partially refactored - CAUSING BUILD ERRORS
- ❌ **Other Services**: Not yet refactored

---

## 🔧 Immediate Fix Required

### Option 1: Complete GuideService Refactoring (Recommended)
Replace ALL remaining `_context` references in GuideService with `_uow` equivalents.

**Pattern to follow**:
```csharp
// OLD
var guide = await _context.Set<Guide>().FirstOrDefaultAsync(g => g.Id == id);
await _context.SaveChangesAsync();

// NEW
var guide = await _uow.Repository<Guide>().FirstOrDefaultAsync(g => g.Id == id);
await _uow.SaveChangesAsync();
```

### Option 2: Revert GuideService (Quick Fix)
Revert GuideService back to using `ApplicationDbContext` temporarily.

---

## 📋 Errors to Fix in GuideService

All these need to be changed from `_context` to `_uow`:

### 1. Simple Queries (58 occurrences)
```csharp
// Line 78, 97, 119, 133, 145, 162, 199, 247, 264, 276, 302, 316, 330, 344, 361, 392, 433, 448, 462, 493, 511, 531
_context.Set<Entity>()  →  _uow.Repository<Entity>().GetQueryable()
```

### 2. Add Operations
```csharp
// Line 424, 484, 380, 412
_context.Set<Entity>().Add(entity)  →  await _uow.Repository<Entity>().AddAsync(entity)
await _context.Set<Entity>().AddAsync(entity)  →  await _uow.Repository<Entity>().AddAsync(entity)
```

### 3. Remove Operations
```csharp
// Line 125, 373, 404, 454, 520
_context.Set<Entity>().Remove(entity)  →  _uow.Repository<Entity>().Delete(entity)
```

### 4. SaveChanges
```csharp
// Line 111, 126, 309, 323, 337, 351, 384, 417, 425, 440, 455, 485, 503, 521
await _context.SaveChangesAsync()  →  await _uow.SaveChangesAsync()
```

### 5. Complex Queries with Include
```csharp
// Line 133-136, 145-148, 162-165, 199-203, 276-279, 531-534
var query = _context.Set<Entity>()
    .Include(e => e.Related)
    .Where(...)

// BECOMES
var query = _uow.Repository<Entity>().GetQueryable()
    .Include(e => e.Related)
    .Where(...)
```

---

## 🚀 Quick Fix Script

Here's what needs to happen in GuideService.cs:

1. **Already Done** ✅:
   - Constructor changed to use `IUnitOfWork _uow`
   - CreateGuideAsync method refactored

2. **Still Needed** ❌:
   - UpdateGuideAsync (lines 77-116)
   - DeleteGuideAsync (lines 118-130)
   - GetGuideByIdAsync (lines 132-142)
   - GetGuideBySlugAsync (lines 144-153)
   - GetGuidesAsync (lines 155-196)
   - GetUserGuidesAsync (lines 198-215)
   - GetCategoriesAsync (lines 217-262)
   - GetCategoryCountsAsync (lines 263-270)
   - SearchGuidesAsync (lines 272-298)
   - PublishGuideAsync (lines 301-313)
   - ArchiveGuideAsync (lines 315-327)
   - SubmitForReviewAsync (lines 329-341)
   - IncrementViewsAsync (lines 343-353)
   - IncrementViewCountAsync (lines 355-358)
   - ToggleLikeAsync (lines 360-388)
   - ToggleBookmarkAsync (lines 391-419)
   - AddStepAsync (lines 421-430)
   - UpdateStepAsync (lines 432-445)
   - DeleteStepAsync (lines 447-459)
   - GetGuideStepsAsync (lines 461-479)
   - AddCommentAsync (lines 481-490)
   - UpdateCommentAsync (lines 492-508)
   - DeleteCommentAsync (lines 510-525)
   - GetGuideCommentsAsync (lines 527-560)
   - MapToDtoAsync (lines 562-600)

---

## 💡 Recommended Action Plan

### Immediate (Next 10 minutes)
1. **Option A**: Revert GuideService to use `ApplicationDbContext` (quick fix to unblock build)
2. **Option B**: Complete GuideService refactoring using ReviewService as template

### Short Term (Today)
1. Build and test the application
2. Verify ReviewService is working correctly
3. Plan systematic refactoring of remaining services

### Medium Term (This Week)
1. Refactor high-priority services one at a time:
   - PostService
   - FriendshipService  
   - QuestionService
2. Test each service after refactoring
3. Keep build green at all times

---

## 📚 Reference: ReviewService (Working Example)

ReviewService is fully refactored and working. Use it as a template:
- Location: `src/CommunityCar.Infrastructure/Services/Community/ReviewService.cs`
- Pattern: All `_context` replaced with `_uow`
- Status: ✅ Building successfully

---

## 🎓 Key Learnings

1. **Don't partially refactor** - Complete one service fully before moving to next
2. **Test after each service** - Keep build green
3. **Use ReviewService as template** - It's the working reference
4. **Follow the pattern consistently** - See architecture-refactoring-guide.md

---

## ⏭️ Next Steps

**Choose ONE**:

### Path A: Quick Fix (Revert)
```bash
# Revert GuideService changes
git checkout src/CommunityCar.Infrastructure/Services/Community/GuideService.cs
# Build should pass
dotnet build
```

### Path B: Complete Fix (Finish Refactoring)
1. Open GuideService.cs
2. Replace all `_context.Set<T>()` with `_uow.Repository<T>()`
3. Replace all `_context.SaveChangesAsync()` with `_uow.SaveChangesAsync()`
4. For complex queries, use `_uow.Repository<T>().GetQueryable()`
5. Build and test

---

## 📞 Status

- **Current**: ❌ BUILD FAILING (58 errors)
- **Blocker**: GuideService partial refactoring
- **Impact**: Cannot build or run application
- **Priority**: 🔴 CRITICAL - Fix immediately

---

**Created**: 2026-02-10
**Status**: ACTIVE - Requires immediate attention
