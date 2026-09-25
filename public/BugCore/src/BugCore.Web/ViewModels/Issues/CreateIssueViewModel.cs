using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using BugCore.Core.Entities;
using BugCore.Core.Enums;

namespace BugCore.Web.ViewModels.Issues;

public class CreateIssueViewModel
{
    [Required]
    [Display(Name = "Project")]
    public int ProjectId { get; set; }

    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    [Display(Name = "Reproducibility")]
    public ReproducibilityType Reproducibility { get; set; } = ReproducibilityType.Always;

    [Display(Name = "Severity")]
    public IssueSeverity Severity { get; set; } = IssueSeverity.Minor;

    [Display(Name = "Priority")]
    public IssuePriority Priority { get; set; } = IssuePriority.Normal;

    [Display(Name = "Assign To")]
    public int? HandlerId { get; set; }

    [Required(ErrorMessage = "Summary is a required field.")]
    [StringLength(250)]
    [Display(Name = "Summary")]
    public string Summary { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is a required field.")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Steps to Reproduce")]
    public string? StepsToReproduce { get; set; }

    [Display(Name = "Additional Information")]
    public string? AdditionalInformation { get; set; }

    [Display(Name = "Product Version")]
    public string? ProductVersion { get; set; }

    [Display(Name = "Product Build")]
    public string? Build { get; set; }

    [Display(Name = "Target Version")]
    public string? TargetVersion { get; set; }

    [Display(Name = "View State")]
    public ProjectViewState ViewState { get; set; } = ProjectViewState.Public;

    [Display(Name = "Tag / Labels")]
    public string? Tags { get; set; }

    [Display(Name = "File Attachment")]
    public IFormFile? Attachment { get; set; }

    [Display(Name = "Attachment Description")]
    public string? AttachmentDescription { get; set; }

    // Custom fields dictionary: CustomFieldId -> Value
    public Dictionary<int, string> CustomFields { get; set; } = new();

    // Dropdowns and lists
    public IEnumerable<Project> Projects { get; set; } = new List<Project>();
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    public IEnumerable<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public IEnumerable<ProjectVersion> Versions { get; set; } = new List<ProjectVersion>();
    public IEnumerable<CustomFieldProject> LinkedCustomFields { get; set; } = new List<CustomFieldProject>();
}
