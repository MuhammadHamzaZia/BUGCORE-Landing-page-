using BugCore.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace BugCore.Web.ViewModels.Issues;

public class ReminderViewModel
{
    public int IssueId { get; set; }
    public Issue? Issue { get; set; }

    [Required(ErrorMessage = "Please select at least one recipient user.")]
    public List<int> RecipientUserIds { get; set; } = new();

    [Required(ErrorMessage = "Reminder body text cannot be empty.")]
    public string ReminderText { get; set; } = string.Empty;

    public IEnumerable<ApplicationUser> AvailableUsers { get; set; } = new List<ApplicationUser>();
}
