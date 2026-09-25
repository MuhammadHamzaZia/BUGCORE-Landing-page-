using BugCore.Core.Entities;
using BugCore.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace BugCore.Web.ViewModels.ManageUser;

public class UserListViewModel
{
    public IEnumerable<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public string? SearchQuery { get; set; }
    public AccessLevel? AccessLevelFilter { get; set; }
}

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    public string? RealName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Reporter;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(4, ErrorMessage = "Password must be at least 4 characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;
    public bool Protected { get; set; } = false;
}

public class EditUserViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    public string UserName { get; set; } = string.Empty;

    public string? RealName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Reporter;

    public bool Enabled { get; set; } = true;
    public bool Protected { get; set; } = false;
}
