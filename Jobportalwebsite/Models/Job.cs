using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jobportalwebsite.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public string? JobTitle { get; set; }
        public string? Description { get; set; }
        public int? CompanyId { get; set; }
        [ForeignKey(nameof(CompanyId))]
        public virtual Company? Company { get; set; }
        public string? RequiredSkills { get; set; } 
        public string? Location { get; set; } 
        public string? EmploymentType { get; set; } 
        public decimal? Salary { get; set; }
        [Required(ErrorMessage = "Salary period is required")]
        public SalaryPeriod? SalaryPeriod { get; set; }
        public DateTime DatePosted { get; set; } = DateTime.Now;
        public JobPostStatus PostStatus { get; set; } = JobPostStatus.Pending;
        public virtual ICollection<JobSkillTest> SkillTests { get; set; } = new List<JobSkillTest>();

        public string? ImageUrl { get; set; } 
    }

    public enum JobPostStatus
    {
        Pending = 1,
        Posted = 2,
        Declined  = 3,
    }

    public enum SalaryPeriod
    {
        PerHour = 1,
        PerDay = 2,
        PerWeek = 3,
        PerMonth = 4,
        PerYear = 5,
        Contract = 6,
        Negotiable = 7
    }
}





