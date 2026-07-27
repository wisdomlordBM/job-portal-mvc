using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jobportalwebsite.Models
{
    public class Application
    {
        public int Id { get; set; }
        public DateTime DateApplied { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
        public string? Contact { get; set; }
        public string? EducationLevel { get; set; }
        public string? FieldOfStudy { get; set; }
        public string? SchoolName { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? Country { get; set; }
        public string? EmploymentType { get; set; }
        [NotMapped]
        public IFormFile? CV { get; set; }

        public string? CVPath { get; set; } // Store the path to the uploaded CV file

        public int JobId { get; set; }
        [ForeignKey(nameof(JobId))]
        public virtual Job? Job { get; set; }
        public string? PerformanceBadge { get; set; } // "Success", "Medium", "Low"
        public int? TestScore { get; set; } // Stores test score (0-100)
        public ICollection<JobSeekerAnswer>? JobSeekerAnswers { get; set; } // Add this line
     
        public string? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; } // Assuming ApplicationUser is your custom user model
    }
}

