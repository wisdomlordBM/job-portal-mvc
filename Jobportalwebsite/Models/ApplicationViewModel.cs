



namespace Jobportalwebsite.Models
{
    public class ApplicationViewModel
    {
        public string? JobTitle { get; set; }
        public int? JobId { get; set; }
        public string? ImageUrl { get; set; }
        public int CompanyId { get; set; }
        public int Id { get; set; }
        public Company? Company { get; set; }
        public string? EmploymentType { get; set; }
        public string? Location { get; set; }
        public decimal? Salary { get; set; }
        public SalaryPeriod? SalaryPeriod { get; set; }
        public string? CurrencySymbol { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EducationLevel { get; set; }
        public string? Contact { get; set; }
        public string? Description { get; set; }
        public DateTime DateApplied { get; set; }
        public string? CVPath { get; set; } // Add the CVPath property
        public string? UserId { get; set; }
        // Added Fields for Test Results
        public int? TestScore { get; set; } // Test Score (0-100)
        public string? PerformanceBadge { get; set; } // "Success", "Medium", "Low"
        public ApplicationUser? User { get; set; }
        //public ApplicationUser? Job { get; set; }
        public Job? Job { get; set; }
        public ICollection<JobSeekerAnswer>? JobSeekerAnswers { get; set; } // Add this line
        public string? ProfilePicturePath { get; set; }

        public string? City { get; set; }
    }

}
