using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.groups;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Web.ViewModels.Groups;
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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GroupsController> _logger;
    private readonly IStringLocalizer<GroupsController> _localizer;

    public GroupsController(
        IGroupService groupService,
        UserManager<ApplicationUser> userManager,
        ILogger<GroupsController> logger,
        IStringLocalizer<GroupsController> localizer)
    {
        _groupService = groupService;
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
            TempData["Error"] = _localizer["FailedToLoadGroups"];
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
            TempData["Error"] = _localizer["FailedToLoadMyGroups"];
            return View(new List<GroupViewModel>());
        }
    }

    [HttpGet("Details/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> Details(string slug, int page = 1, int pageSize = 20)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var group = await _groupService.GetGroupBySlugAsync(slug, userId);

            if (group == null)
            {
                TempData["Error"] = _localizer["GroupNotFound"];
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

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading group details for slug: {Slug}", slug);
            TempData["Error"] = _localizer["FailedToLoadGroup"];
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
                TempData["Success"] = _localizer["GroupCreated"];
                return RedirectToAction(nameof(Details), new { slug = result.Value!.Slug });
            }

            ModelState.AddModelError("", result.Error ?? _localizer["FailedToCreateGroup"]);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            ModelState.AddModelError("", _localizer["FailedToCreateGroupDetail"]);
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
                TempData["Error"] = "Group not found.";
                return RedirectToAction(nameof(Index));
            }

            if (group.UserRole != GroupMemberRole.Admin && group.CreatorId != userId)
            {
                TempData["Error"] = _localizer["NoPermissionToEditGroup"];
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
            TempData["Error"] = _localizer["FailedToLoadGroup"];
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
                TempData["Success"] = _localizer["GroupUpdated"];
                return RedirectToAction(nameof(Details), new { slug = result.Value!.Slug });
            }

            ModelState.AddModelError("", result.Error ?? _localizer["FailedToUpdateGroup"]);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group: {GroupId}", id);
            ModelState.AddModelError("", _localizer["FailedToUpdateGroupDetail"]);
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
                TempData["Success"] = _localizer["GroupDeleted"];
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = result.Error ?? "Failed to delete group";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group: {GroupId}", id);
            TempData["Error"] = _localizer["FailedToDeleteGroupDetail"];
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
                TempData["Success"] = _localizer["JoinedGroup"];
            }
            else
            {
                TempData["Error"] = result.Error ?? "Failed to join group";
            }

            var group = await _groupService.GetGroupByIdAsync(id, userId);
            return RedirectToAction(nameof(Details), new { slug = group?.Slug ?? id.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining group: {GroupId}", id);
            TempData["Error"] = _localizer["FailedToJoinGroup"];
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Leave/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _groupService.LeaveGroupAsync(id, userId);

            if (result.IsSuccess)
            {
                TempData["Success"] = _localizer["LeftGroup"];
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = result.Error ?? "Failed to leave group";
            var group = await _groupService.GetGroupByIdAsync(id, userId);
            return RedirectToAction(nameof(Details), new { slug = group?.Slug ?? id.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving group: {GroupId}", id);
            TempData["Error"] = _localizer["FailedToLeaveGroup"];
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
                TempData["Success"] = _localizer["MemberRemoved"];
            }
            else
            {
                TempData["Error"] = result.Error ?? "Failed to remove member";
            }

            var group = await _groupService.GetGroupByIdAsync(groupId, userId);
            return RedirectToAction(nameof(Details), new { slug = group?.Slug ?? groupId.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from group: {GroupId}", groupId);
            TempData["Error"] = _localizer["FailedToRemoveMember"];
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
                TempData["Success"] = _localizer["MemberRoleUpdated"];
            }
            else
            {
                TempData["Error"] = result.Error ?? "Failed to update member role";
            }

            var group = await _groupService.GetGroupByIdAsync(groupId, userId);
            return RedirectToAction(nameof(Details), new { slug = group?.Slug ?? groupId.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member role in group: {GroupId}", groupId);
            TempData["Error"] = _localizer["FailedToUpdateMemberRole"];
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
            TempData["Error"] = _localizer["FailedToSearchGroups"];
            return RedirectToAction(nameof(Index));
        }
    }
}
