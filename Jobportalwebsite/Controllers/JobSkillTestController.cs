using Jobportalwebsite.Data;
using Jobportalwebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Jobportalwebsite.Controllers
{
    public class JobSkillTestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobSkillTestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create(int jobId)
        {
            ViewBag.JobId = jobId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int jobId, List<JobSkillTest> questions)
        {
            if (questions.Count < 5)
            {
                TempData["ErrorMessage"] = "You must add at least 5 questions.";
                return View(questions);
            }

            foreach (var question in questions)
            {
                question.JobId = jobId;
                _context.JobSkillTests.Add(question);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Questions added successfully!";
            return RedirectToAction("Index", "Company", new { id = jobId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTest(int jobId, Dictionary<int, string> answers)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var job = _context.Jobs.Include(j => j.SkillTests).FirstOrDefault(j => j.Id == jobId);

            if (job == null)
            {
                return NotFound();
            }

            var correctAnswers = job.SkillTests.ToDictionary(q => q.Id, q => q.CorrectAnswer);
            int totalQuestions = correctAnswers.Count;
            int correctCount = answers.Count(a => correctAnswers.ContainsKey(a.Key) && correctAnswers[a.Key] == a.Value);

            int score = (int)((double)correctCount / totalQuestions * 100);

            // Assign badge based on score
            string badge = score >= 80 ? "Success" : (score >= 50 ? "Medium" : "Low");

            // Update application with test score and badge
            var application = _context.Applications.FirstOrDefault(a => a.JobId == jobId && a.UserId == userId);
            if (application != null)
            {
                application.TestScore = score;
                application.PerformanceBadge = badge;
                _context.Update(application);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Test submitted successfully!";
            return RedirectToAction("Index", "Job");
        }

    }

}
