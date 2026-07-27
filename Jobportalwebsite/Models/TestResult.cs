using System;

namespace Jobportalwebsite.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SkillTestId { get; set; }
        public int Score { get; set; }
        public int? CompletionTime { get; set; } // Time in seconds
        public bool BadgeEarned { get; set; } = false;
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public virtual SkillTest? SkillTest { get; set; }
    }
}

