using System;
using System.ComponentModel.DataAnnotations;

namespace Jobportalwebsite.Viewmodel
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }

        // These fields are now optional.
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? Gender { get; set; }

        public string? Address { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; } // Note: changed to nullable DateTime
    }
}


