using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Web.Areas.Communications.ViewModels;

public class NotificationListViewModel
{
    public List<NotificationItemViewModel> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool? UnreadOnly { get; set; }
}

public class NotificationItemViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

public class NotificationDropdownViewModel
{
    public List<NotificationItemViewModel> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
}

public class CreateNotificationViewModel
{
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
    public string Message { get; set; } = string.Empty;

    [Url(ErrorMessage = "Invalid URL format")]
    [StringLength(500, ErrorMessage = "Link cannot exceed 500 characters")]
    public string? Link { get; set; }
}

public class NotificationSettingsViewModel
{
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool QuestionNotifications { get; set; }
    public bool AnswerNotifications { get; set; }
    public bool CommentNotifications { get; set; }
    public bool VoteNotifications { get; set; }
    public bool FriendRequestNotifications { get; set; }
    public bool MessageNotifications { get; set; }
}
