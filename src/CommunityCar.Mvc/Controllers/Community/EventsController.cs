using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.events;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.ViewModels.Events;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Community;

[Route("{culture:alpha}/Events")]
public class EventsController : Controller
{
    private readonly IEventService _eventService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<EventsController> _logger;
    private readonly IStringLocalizer<EventsController> _localizer;

    public EventsController(
        IEventService eventService,
        ICurrentUserService currentUserService,
        ILogger<EventsController> logger,
        IStringLocalizer<EventsController> localizer)
    {
        _eventService = eventService;
        _currentUserService = currentUserService;
        _logger = logger;
        _localizer = localizer;
    }

    // GET: Events
    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1, 
        int pageSize = 12, 
        EventCategory? category = null,
        bool upcoming = true)
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var currentUserId = GetCurrentUserId();

            PagedResult<Domain.DTOs.Community.EventDto> result;
            
            if (upcoming)
            {
                result = await _eventService.GetUpcomingEventsAsync(parameters, currentUserId);
            }
            else
            {
                result = await _eventService.GetEventsAsync(
                    parameters, 
                    category: category,
                    status: EventStatus.Published,
                    currentUserId: currentUserId);
            }

            ViewBag.CurrentCategory = category;
            ViewBag.IsUpcoming = upcoming;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Categories = Enum.GetValues<EventCategory>();

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading events");
            TempData["Error"] = _localizer["FailedToLoadEvents"].Value;
            return View(new PagedResult<Domain.DTOs.Community.EventDto>(
                new List<Domain.DTOs.Community.EventDto>(), 0, page, pageSize));
        }
    }

    // GET: Events/Details/{slug}
    [HttpGet("Details/{slug}")]
    [HttpGet("Details")]
    public async Task<IActionResult> Details(string slug)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var eventDto = await _eventService.GetEventBySlugAsync(slug, currentUserId);

            if (eventDto == null)
            {
                TempData["Error"] = "Event not found";
                return RedirectToAction(nameof(Index));
            }

            var attendeesParams = new QueryParameters { PageNumber = 1, PageSize = 20 };
            var attendees = await _eventService.GetEventAttendeesAsync(
                eventDto.Id, 
                attendeesParams, 
                AttendeeStatus.Going);

            var commentsParams = new QueryParameters { PageNumber = 1, PageSize = 10 };
            var comments = await _eventService.GetEventCommentsAsync(
                eventDto.Id, 
                commentsParams, 
                currentUserId);

            var viewModel = new EventDetailsViewModel
            {
                Event = eventDto,
                Attendees = attendees,
                Comments = comments,
                RelatedEvents = new List<Domain.DTOs.Community.EventDto>()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading event {Slug}", slug);
            TempData["Error"] = _localizer["FailedToLoadEvent"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Events/Create
    [Authorize]
    [HttpGet("Create")]
    public IActionResult Create()
    {
        ViewBag.Categories = Enum.GetValues<EventCategory>();
        return View();
    }

    // POST: Events/Create
    [Authorize]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEventViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = Enum.GetValues<EventCategory>();
                return View(model);
            }

            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();

            var communityEvent = await _eventService.CreateEventAsync(
                model.Title,
                model.Description,
                model.StartTime,
                model.EndTime,
                model.Location,
                userId,
                model.Category,
                model.MaxAttendees,
                model.IsOnline);

            TempData["Success"] = _localizer["EventCreated"].Value;
            return RedirectToAction(nameof(Details), new { slug = communityEvent.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            ModelState.AddModelError("", _localizer["FailedToCreateEvent"].Value);
            ViewBag.Categories = Enum.GetValues<EventCategory>();
            return View(model);
        }
    }

    // GET: Events/Edit/{id}
    [Authorize]
    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            var eventDto = await _eventService.GetEventByIdAsync(id, userId);

            if (eventDto == null)
            {
                TempData["Error"] = "Event not found";
                return RedirectToAction(nameof(Index));
            }

            if (!eventDto.IsOrganizer)
            {
                TempData["Error"] = _localizer["OnlyOrganizerCanEdit"].Value;
                return RedirectToAction(nameof(Details), new { slug = eventDto.Slug });
            }

            var model = new EditEventViewModel
            {
                Id = eventDto.Id,
                Title = eventDto.Title,
                Description = eventDto.Description,
                StartTime = eventDto.StartTime,
                EndTime = eventDto.EndTime,
                Location = eventDto.Location,
                Address = eventDto.Address,
                Category = eventDto.Category,
                MaxAttendees = eventDto.MaxAttendees,
                IsOnline = eventDto.IsOnline,
                OnlineUrl = eventDto.OnlineUrl,
                ImageUrl = eventDto.ImageUrl
            };

            ViewBag.Categories = Enum.GetValues<EventCategory>();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading event for edit {EventId}", id);
            TempData["Error"] = _localizer["FailedToLoadEvent"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Events/Edit/{id}
    [Authorize]
    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditEventViewModel model)
    {
        try
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = Enum.GetValues<EventCategory>();
                return View(model);
            }

            var communityEvent = await _eventService.UpdateEventAsync(
                id,
                model.Title,
                model.Description,
                model.StartTime,
                model.EndTime,
                model.Location,
                model.Category,
                model.MaxAttendees,
                model.IsOnline);

            TempData["Success"] = _localizer["EventUpdated"].Value;
            return RedirectToAction(nameof(Details), new { slug = communityEvent.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", id);
            ModelState.AddModelError("", _localizer["FailedToUpdateEvent"].Value);
            ViewBag.Categories = Enum.GetValues<EventCategory>();
            return View(model);
        }
    }

    // POST: Events/Delete/{id}
    [Authorize]
    [HttpPost("Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _eventService.DeleteEventAsync(id);
            return Json(new { success = true, message = _localizer["EventDeleted"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", id);
            return Json(new { success = false, message = _localizer["FailedToDeleteEvent"].Value });
        }
    }

    // POST: Events/Join/{id}
    [Authorize]
    [HttpPost("Join/{id:guid}")]
    public async Task<IActionResult> Join(Guid id, AttendeeStatus status = AttendeeStatus.Going)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _eventService.JoinEventAsync(id, userId, status);

            return Json(new { success = true, message = _localizer["JoinedEvent"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining event {EventId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Events/Leave/{id}
    [Authorize]
    [HttpPost("Leave/{id:guid}")]
    public async Task<IActionResult> Leave(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _eventService.LeaveEventAsync(id, userId);

            return Json(new { success = true, message = _localizer["LeftEvent"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving event {EventId}", id);
            return Json(new { success = false, message = _localizer["FailedToLeaveEvent"].Value });
        }
    }

    // POST: Events/Publish/{id}
    [Authorize]
    [HttpPost("Publish/{id:guid}")]
    public async Task<IActionResult> Publish(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _eventService.PublishEventAsync(id, userId);

            return Json(new { success = true, message = _localizer["EventPublished"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Events/Cancel/{id}
    [Authorize]
    [HttpPost("Cancel/{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _eventService.CancelEventAsync(id, userId);

            return Json(new { success = true, message = _localizer["EventCancelled"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling event {EventId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Events/AddComment
    [Authorize]
    [HttpPost("AddComment")]
    public async Task<IActionResult> AddComment(Guid eventId, string content)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _eventService.AddCommentAsync(eventId, userId, content);

            return Json(new { success = true, message = _localizer["CommentAdded"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to event {EventId}", eventId);
            return Json(new { success = false, message = _localizer["FailedToAddComment"].Value });
        }
    }

    // GET: Events/MyEvents
    [Authorize]
    [HttpGet("MyEvents")]
    public async Task<IActionResult> MyEvents(int page = 1, int pageSize = 12, bool asOrganizer = false)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var result = await _eventService.GetUserEventsAsync(userId, parameters, asOrganizer);

            ViewBag.AsOrganizer = asOrganizer;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user events");
            TempData["Error"] = _localizer["FailedToLoadMyEvents"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    private Guid? GetCurrentUserId()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return null;

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

