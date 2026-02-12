using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.groups;
using CommunityCar.Domain.Enums.Community.post;
using CommunityCar.Domain.Enums.Community.reviews;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.ViewModels.Groups;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Community;

[Route("{culture:alpha}/[controller]")]
[Authorize]
public partial class GroupsController : Controller
{
    private readonly IGroupService _groupService;
    private readonly IPostService _postService;
    private readonly IQuestionService _questionService;
    private readonly IReviewService _reviewService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GroupsController> _logger;
    private readonly IStringLocalizer<GroupsController> _localizer;

    public GroupsController(
        IGroupService groupService,
        IPostService postService,
        IQuestionService questionService,
        IReviewService reviewService,
        UserManager<ApplicationUser> userManager,
        ILogger<GroupsController> logger,
        IStringLocalizer<GroupsController> localizer)
    {
        _groupService = groupService;
        _postService = postService;
        _questionService = questionService;
        _reviewService = reviewService;
        _userManager = userManager;
        _logger = logger;
        _localizer = localizer;
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        return userId;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 12)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var result = await _groupService.GetGroupsAsync(parameters, userId);

            var viewModels = result.Items.Select(g => new GroupViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Slug = g.Slug,
                IsPrivate = g.IsPrivate,
                CreatorId = g.CreatorId,
                CreatorName = g.CreatorName,
                ImageUrl = g.ImageUrl,
                MemberCount = g.MemberCount,
                CreatedAt = g.CreatedAt,
                IsMember = g.IsUserMember,
                IsAdmin = g.UserRole == GroupMemberRole.Admin
            }).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalCount = result.TotalCount;

            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading groups");
            TempData["Error"] = _localizer["FailedToLoadGroups"].Value;
            return View(new List<GroupViewModel>());
        }
    }

    [HttpGet("MyGroups")]
    public async Task<IActionResult> MyGroups(int page = 1, int pageSize = 12)
    {
        try
        {
            var userId = GetCurrentUserId();
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var result = await _groupService.GetUserGroupsAsync(userId, parameters);

            var viewModels = result.Items.Select(g => new GroupViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Slug = g.Slug,
                IsPrivate = g.IsPrivate,
                CreatorId = g.CreatorId,
                CreatorName = g.CreatorName,
                ImageUrl = g.ImageUrl,
                MemberCount = g.MemberCount,
                CreatedAt = g.CreatedAt,
                IsMember = g.IsUserMember,
                IsAdmin = g.UserRole == GroupMemberRole.Admin
            }).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalCount = result.TotalCount;

            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user groups");
            TempData["Error"] = _localizer["FailedToLoadMyGroups"].Value;
            return View(new List<GroupViewModel>());
        }
    }

    [HttpGet("Details/{slug}")]
    [HttpGet("Details")]
    [AllowAnonymous]
    public async Task<IActionResult> Details(string slug, string tab = "about", int page = 1, int pageSize = 20)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var group = await _groupService.GetGroupBySlugAsync(slug, userId);

            if (group == null)
            {
                TempData["Error"] = _localizer["GroupNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new GroupViewModel
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                Slug = group.Slug,
                IsPrivate = group.IsPrivate,
                CreatorId = group.CreatorId,
                CreatorName = group.CreatorName,
                ImageUrl = group.ImageUrl,
                MemberCount = group.MemberCount,
                CreatedAt = group.CreatedAt,
                IsMember = group.IsUserMember,
                IsAdmin = group.UserRole == GroupMemberRole.Admin
            };

            // Get members
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var membersResult = await _groupService.GetGroupMembersAsync(group.Id, parameters);

            ViewBag.Members = membersResult.Items.Select(m => new GroupMemberViewModel
            {
                Id = m.Id,
                UserId = m.UserId,
                UserName = m.UserName,
                ProfilePictureUrl = m.ProfilePictureUrl,
                UserSlug = m.Slug,
                Role = (int)m.Role,
                RoleName = m.Role.ToString(),
                JoinedAt = m.JoinedAt
            }).ToList();

            ViewBag.MembersPage = page;
            ViewBag.MembersTotalPages = membersResult.TotalPages;
            ViewBag.ActiveTab = tab.ToLower();

            // Get group posts
            var postsParams = new QueryParameters { PageNumber = 1, PageSize = 10 };
            var postsResult = await _postService.GetPostsAsync(
                postsParams,
                PostStatus.Published,
                null,
                group.Id,
                userId);
            ViewBag.Posts = postsResult;

            // Get group questions
            var questionsParams = new QueryParameters { PageNumber = 1, PageSize = 10 };
            var questionsResult = await _questionService.GetQuestionsAsync(
                questionsParams,
                searchTerm: null,
                tag: null,
                isResolved: null,
                hasAnswers: null,
                categoryId: null,
                currentUserId: userId);
            
            // Filter questions by groupId (if QuestionService doesn't support it yet)
            // For now, we'll show all questions - this will be updated when QuestionService adds groupId support
            ViewBag.Questions = questionsResult;

            // Get group reviews
            var reviewsParams = new QueryParameters { PageNumber = 1, PageSize = 10 };
            var reviewsResult = await _reviewService.GetReviewsAsync(
                reviewsParams,
                null,
                ReviewStatus.Approved,
                null,
                null,
                userId);
            
            // Filter reviews by groupId (if ReviewService doesn't support it yet)
            // For now, we'll show all reviews - this will be updated when ReviewService adds groupId support
            ViewBag.Reviews = reviewsResult;

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading group details for slug: {Slug}", slug);
            TempData["Error"] = _localizer["FailedToLoadGroup"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new CreateGroupViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGroupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.CreateGroupAsync(
                model.Name,
                model.Description,
                userId,
                model.IsPrivate);

            if (result.IsSuccess)
            {
                TempData["Success"] = _localizer["GroupCreated"].Value;
                return RedirectToAction(nameof(Details), new { slug = result.Value!.Slug });
            }

            ModelState.AddModelError("", result.Error ?? _localizer["FailedToCreateGroup"].Value);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            ModelState.AddModelError("", _localizer["FailedToCreateGroupDetail"].Value);
            return View(model);
        }
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var group = await _groupService.GetGroupByIdAsync(id, userId);

            if (group == null)
            {
                TempData["Error"] = _localizer["GroupNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            if (group.UserRole != GroupMemberRole.Admin && group.CreatorId != userId)
            {
                TempData["Error"] = _localizer["NoPermissionToEditGroup"].Value;
                return RedirectToAction(nameof(Details), new { slug = group.Slug });
            }

            var viewModel = new EditGroupViewModel
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                IsPrivate = group.IsPrivate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading group for edit: {GroupId}", id);
            TempData["Error"] = _localizer["FailedToLoadGroup"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditGroupViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.UpdateGroupAsync(
                model.Id,
                model.Name,
                model.Description,
                model.IsPrivate,
                userId);

            if (result.IsSuccess)
            {
                TempData["Success"] = _localizer["GroupUpdated"].Value;
                return RedirectToAction(nameof(Details), new { slug = result.Value!.Slug });
            }

            ModelState.AddModelError("", result.Error ?? _localizer["FailedToUpdateGroup"].Value);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group: {GroupId}", id);
            ModelState.AddModelError("", _localizer["FailedToUpdateGroupDetail"].Value);
            return View(model);
        }
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.DeleteGroupAsync(id, userId);

            if (result.IsSuccess)
            {
                TempData["Success"] = _localizer["GroupDeleted"].Value;
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = result.Error ?? _localizer["FailedToDeleteGroup"].Value;
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group: {GroupId}", id);
            TempData["Error"] = _localizer["FailedToDeleteGroupDetail"].Value;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost("Join/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.JoinGroupAsync(id, userId);

            if (result.IsSuccess)
            {
                TempData["Success"] = _localizer["JoinedGroup"].Value;
            }
            else
            {
                TempData["Error"] = result.Error ?? _localizer["FailedToJoinGroup"].Value;
            }

            var group = await _groupService.GetGroupByIdAsync(id, userId);
            return RedirectToAction(nameof(Details), new { slug = group?.Slug ?? id.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining group: {GroupId}", id);
            TempData["Error"] = _localizer["FailedToJoinGroup"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Leave/{id}")]
    public async Task<IActionResult> Leave(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var group = await _groupService.GetGroupByIdAsync(id, userId);

            if (group == null)
            {
                TempData["Error"] = _localizer["GroupNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            return View(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading leave group page: {GroupId}", id);
            TempData["Error"] = _localizer["FailedToLoadGroup"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Leave/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LeaveConfirmed(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.LeaveGroupAsync(id, userId);

            if (result.IsSuccess)
            {
                TempData["Success"] = _localizer["LeftGroup"].Value;
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = result.Error ?? _localizer["FailedToLeaveGroup"].Value;
            var group = await _groupService.GetGroupByIdAsync(id, userId);
            return RedirectToAction(nameof(Details), new { slug = group?.Slug ?? id.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving group: {GroupId}", id);
            TempData["Error"] = _localizer["FailedToLeaveGroup"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("RemoveMember")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(Guid groupId, Guid memberId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.RemoveMemberAsync(groupId, memberId, userId);

            if (result.IsSuccess)
            {
                TempData["Success"] = _localizer["MemberRemoved"].Value;
            }
            else
            {
                TempData["Error"] = result.Error ?? _localizer["FailedToRemoveMember"].Value;
            }

            var group = await _groupService.GetGroupByIdAsync(groupId, userId);
            return RedirectToAction(nameof(Details), new { slug = group?.Slug ?? groupId.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from group: {GroupId}", groupId);
            TempData["Error"] = _localizer["FailedToRemoveMember"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("UpdateRole")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRole(Guid groupId, Guid memberId, int role)
    {
        try
        {
            var userId = GetCurrentUserId();
            var memberRole = (GroupMemberRole)role;
            var result = await _groupService.UpdateMemberRoleAsync(groupId, memberId, memberRole, userId);

            if (result.IsSuccess)
            {
                TempData["Success"] = _localizer["MemberRoleUpdated"].Value;
            }
            else
            {
                TempData["Error"] = result.Error ?? _localizer["FailedToUpdateMemberRole"].Value;
            }

            var group = await _groupService.GetGroupByIdAsync(groupId, userId);
            return RedirectToAction(nameof(Details), new { slug = group?.Slug ?? groupId.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member role in group: {GroupId}", groupId);
            TempData["Error"] = _localizer["FailedToUpdateMemberRole"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search(string query, int page = 1, int pageSize = 12)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction(nameof(Index));
            }

            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var result = await _groupService.SearchGroupsAsync(query, parameters, userId);

            var viewModels = result.Items.Select(g => new GroupViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Slug = g.Slug,
                IsPrivate = g.IsPrivate,
                CreatorId = g.CreatorId,
                CreatorName = g.CreatorName,
                ImageUrl = g.ImageUrl,
                MemberCount = g.MemberCount,
                CreatedAt = g.CreatedAt,
                IsMember = g.IsUserMember,
                IsAdmin = g.UserRole == GroupMemberRole.Admin
            }).ToList();

            ViewBag.SearchQuery = query;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalCount = result.TotalCount;

            return View("Index", viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching groups with query: {Query}", query);
            TempData["Error"] = _localizer["FailedToSearchGroups"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Groups/GetGroupPosts/{id}
    [HttpGet("GetGroupPosts/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGroupPosts(Guid id, int page = 1, int pageSize = 10)
    {
        try
        {
            // This will be called via AJAX to load group posts
            // For now, return empty result - will be implemented when PostService is injected
            return Json(new { success = true, posts = new List<object>(), totalPages = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading group posts for group: {GroupId}", id);
            return Json(new { success = false, message = _localizer["FailedToLoadGroupPosts"].Value });
        }
    }

    // GET: Groups/GetGroupQuestions/{id}
    [HttpGet("GetGroupQuestions/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGroupQuestions(Guid id, int page = 1, int pageSize = 10)
    {
        try
        {
            // This will be called via AJAX to load group questions
            // For now, return empty result - will be implemented when QuestionService is injected
            return Json(new { success = true, questions = new List<object>(), totalPages = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading group questions for group: {GroupId}", id);
            return Json(new { success = false, message = _localizer["FailedToLoadGroupQuestions"].Value });
        }
    }

    // GET: Groups/GetGroupReviews/{id}
    [HttpGet("GetGroupReviews/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGroupReviews(Guid id, int page = 1, int pageSize = 10)
    {
        try
        {
            // This will be called via AJAX to load group reviews
            // For now, return empty result - will be implemented when ReviewService is injected
            return Json(new { success = true, reviews = new List<object>(), totalPages = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading group reviews for group: {GroupId}", id);
            return Json(new { success = false, message = _localizer["FailedToLoadGroupReviews"].Value });
        }
    }
}
