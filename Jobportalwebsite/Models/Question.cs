using System;

namespace Jobportalwebsite.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int SkillTestId { get; set; }
        public string? QuestionText { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? CorrectAnswer { get; set; } // "A", "B", etc.

        public virtual SkillTest? SkillTest { get; set; }
    }
}

