namespace Jobportalwebsite.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty; // Initialized to avoid null
        public string Email { get; set; } = string.Empty; // Initialized to avoid null
        public string Password { get; set; } // Add this line
        public string PasswordHash { get; set; } = string.Empty; // Initialized to avoid null
        public string Role { get; set; } = string.Empty; // e.g., JobSeeker, Employer
        public DateTime DateJoined { get; set; } = DateTime.UtcNow; // Default to current date
        public string Resume { get; set; } = string.Empty; // Job Seekers can upload a resume
        public string CompanyName { get; set; } = string.Empty; // Employers can add a company name
    }
}


