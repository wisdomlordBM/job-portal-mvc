using System;
using System.ComponentModel.DataAnnotations;

namespace Jobportalwebsite.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Industry { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? Phone { get; set; }
        public int? EmployerId { get; set; }

        // Branding
        public string? ProfilePicturePath { get; set; } // Logo
        public string? CoverBannerPath { get; set; }     // Cover banner

        // Office Information
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        public int? CountryId { get; set; }

        // Tracks onboarding wizard progress
        public CompanyOnboardingStep OnboardingStep { get; set; } = CompanyOnboardingStep.Details;

        public virtual User? Employer { get; set; }
        public virtual Country? Country { get; set; }
    }

    public enum CompanyOnboardingStep
    {
        Details = 1,     // Step 2: Company Details not yet completed
        Branding = 2,    // Step 3: Branding not yet completed
        OfficeInfo = 3,  // Step 4: Office Information not yet completed
        Completed = 4    // Wizard finished — full dashboard access
    }
}


