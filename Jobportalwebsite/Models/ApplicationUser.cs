using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jobportalwebsite.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public int? CountryId { get; set; }
        public DateTime? DateCreated { get; set; }
        public string? ProfilePicturePath { get; set; }

        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
        public string? Role { get; set; }
        public virtual Country? CountryReference { get; set; }

        public bool IsProfileComplete()
        {
            return !string.IsNullOrWhiteSpace(FirstName)
                && !string.IsNullOrWhiteSpace(LastName)
                && !string.IsNullOrWhiteSpace(PhoneNumber)
                && !string.IsNullOrWhiteSpace(Address)
                && CountryId.HasValue
                && !string.IsNullOrWhiteSpace(Gender)
                && !string.IsNullOrWhiteSpace(DateOfBirth);
        }
    }

}

