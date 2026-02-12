namespace CommunityCar.Mvc.ViewModels.Notifications;

public class NotificationViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Url { get; set; }
}

public class NotificationPreferencesViewModel
{
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool FriendRequests { get; set; }
    public bool PostComments { get; set; }
    public bool PostLikes { get; set; }
    public bool QuestionAnswers { get; set; }
    public bool EventReminders { get; set; }
    public bool GroupInvitations { get; set; }
}
