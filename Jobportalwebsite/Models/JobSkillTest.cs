namespace Jobportalwebsite.Models
{
    public class JobSkillTest
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string? Question { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? CorrectAnswer { get; set; } // e.g., "A", "B", "C", "D"

        public Job? Job { get; set; }
        public ICollection<JobSeekerAnswer>? JobSeekerAnswers { get; set; } // Add this line

    }

}
