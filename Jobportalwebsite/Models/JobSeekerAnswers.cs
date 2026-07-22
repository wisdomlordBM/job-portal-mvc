namespace Jobportalwebsite.Models
{
    public class JobSeekerAnswer
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int JobSkillTestId { get; set; }
        public string? SelectedAnswer { get; set; } // A, B, C, or D

        public Application? Application { get; set; }
        public JobSkillTest? JobSkillTest { get; set; }
    }

}
