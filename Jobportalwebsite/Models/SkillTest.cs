using System;
using System.Collections.Generic;

namespace Jobportalwebsite.Models
{
    public class SkillTest
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Difficulty { get; set; } // Easy, Medium, Hard
        public string? Category { get; set; } // E.g., Programming, Marketing
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Question>? Questions { get; set; }
    }
}
