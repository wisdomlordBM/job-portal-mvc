using Jobportalwebsite.Models;
namespace Jobportalwebsite.ViewModel
{
    public class CompanyDashboardViewModel
    {
        public int CompanyId { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? Industry { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? Phone { get; set; }
        public string? ProfilePicturePath { get; set; }
        public string? CoverBannerPath { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? CountryName { get; set; }
        public string? Email { get; set; }
        public List<JobViewModel>? Jobs { get; set; }
        public int? JobId { get; set; }
    }

    public class JobViewModel
    {
        public int Id { get; set; }
        public string? JobTitle { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
        public decimal? Salary { get; set; }
        public SalaryPeriod? SalaryPeriod { get; set; }
        public string? CurrencySymbol { get; set; }
        public string? ImageUrl { get; set; }
        public JobPostStatus? JobPostStatus { get; set; }
    }
}



