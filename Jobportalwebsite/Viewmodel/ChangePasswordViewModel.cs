using System.ComponentModel.DataAnnotations;

namespace Jobportalwebsite.Viewmodel
{
    public class ChangePasswordViewModel
    {
        // Only required if the account already has a password (not a Google-only login)
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool HasExistingPassword { get; set; }
    }
}