using System.ComponentModel.DataAnnotations;

namespace BugCore.Web.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember login on this computer")]
    public bool RememberMe { get; set; } = false;

    public string? ReturnUrl { get; set; }
}
