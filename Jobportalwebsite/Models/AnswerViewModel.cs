namespace Jobportalwebsite.Models
{
    public class AnswerViewModel
    {
        public string? QuestionText { get; set; } 
        public string? CorrectAnswer { get; set; } 
        public string? UserSelectedAnswer { get; set; }
        public bool IsCorrect => UserSelectedAnswer == CorrectAnswer;
    }
}

