namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels.management;

public class UserViewModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
}

public class CreateUserViewModel
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> AllRoles { get; set; } = new();
}

public class EditUserViewModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public IList<string> CurrentRoles { get; set; } = new List<string>();
    public List<string> SelectedRoles { get; set; } = new();
    public List<string> AllRoles { get; set; } = new();
}
