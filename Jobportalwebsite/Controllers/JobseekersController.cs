using Jobportalwebsite.Data;
using Jobportalwebsite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Linq;
using System;
using Jobportalwebsite.Services;
using System.Threading.Tasks;
using UploadCVViewModel = Jobportalwebsite.Models.UploadCVViewModel;
using Microsoft.Extensions.Hosting;
namespace Jobportalwebsite.Controllers
{
    public class JobseekersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly NotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobseekersController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            NotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _environment = environment;
            _notificationService = notificationService;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Where(u => u.Id == userId).ToList();
            return View(user);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var jobseeker = _context.Jobseekers.Find(id);
            if (jobseeker != null)
            {
                _context.Jobseekers.Remove(jobseeker);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Create(int jobId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User session expired or unauthorized access.";
                return RedirectToAction("Login", "Account");
            }

            var job = _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.SkillTests) // Include SkillTests
                .FirstOrDefault(j => j.Id == jobId);

            var user = _context.Users
                .FirstOrDefault(u => u.Id == userId);

            if (job == null || user == null)
            {
                TempData["ErrorMessage"] = "You must register as a jobseeker to apply for a job.";
                return RedirectToAction("Register", "Account");
            }

            bool hasApplied = _context.Applications
                .Any(a => a.JobId == jobId && a.UserId == userId);

            if (hasApplied)
            {
                TempData["ErrorMessage"] = "You have already applied for this job.";
                return RedirectToAction("Index", "Job");
            }

            ViewBag.Job = job;
            ViewBag.User = user;
            ViewBag.SkillTestQuestions = job.SkillTests?.ToList() ?? new List<JobSkillTest>();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Application application, int jobId, IFormFile CV, Dictionary<int, string> SkillTestAnswers)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingApplication = _context.Applications
                .FirstOrDefault(a => a.JobId == jobId && a.UserId == userId);

            if (existingApplication != null)
            {
                TempData["ErrorMessage"] = "You cannot apply for the same job more than once.";
                return RedirectToAction("Index", "Job");
            }

            if (CV != null && CV.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("CV", "File size exceeds the 5MB limit.");
                ViewBag.CurrentSection = "jobExperience";
                return View(application);
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var fileExtension = Path.GetExtension(CV?.FileName).ToLower();
            if (CV != null && !allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("CV", "Only PDF, DOC, and DOCX files are allowed.");
            }

            if (ModelState.IsValid)
            {
                var job = _context.Jobs
                    .Include(j => j.Company)
                    .Include(j => j.SkillTests)
                    .FirstOrDefault(j => j.Id == jobId);
                if (job == null)
                {
                    return NotFound();
                }

                var myApplication = new Application
                {
                    JobId = jobId,
                    Description = application.Description,
                    Contact = application.Contact,
                    EducationLevel = application.EducationLevel,
                    FieldOfStudy = application.FieldOfStudy,
                    SchoolName = application.SchoolName,
                    City = application.City,
                    State = application.State,
                    JobTitle = job.JobTitle,
                    CompanyName = job.Company.Name,
                    Country = application.Country,
                    EmploymentType = application.EmploymentType,
                    UserId = userId,
                    DateApplied = DateTime.UtcNow
                };

                _context.Applications.Add(myApplication);
                await _context.SaveChangesAsync();

                // Notify the company that a new applicant has applied
                if (job.Company != null && !string.IsNullOrEmpty(job.Company.Email))
                {
                    var companyUser = await _userManager.FindByEmailAsync(job.Company.Email);
                    if (companyUser != null)
                    {
                        var applicant = await _userManager.FindByIdAsync(userId);
                        var applicantName = applicant != null
                            ? $"{applicant.FirstName} {applicant.LastName}".Trim()
                            : string.Empty;

                        var message = string.IsNullOrWhiteSpace(applicantName)
                            ? $"A candidate applied for '{job.JobTitle}'."
                            : $"{applicantName} applied for '{job.JobTitle}'.";

                        await _notificationService.NotifyUserAsync(companyUser.Id, message);
                    }
                }

                // Store skill test answers and calculate test score if answers were provided
                if (SkillTestAnswers != null && SkillTestAnswers.Count > 0)
                {
                    foreach (var answer in SkillTestAnswers)
                    {
                        var testAnswer = new JobSeekerAnswer
                        {
                            ApplicationId = myApplication.Id,
                            JobSkillTestId = answer.Key,
                            SelectedAnswer = answer.Value
                        };
                        _context.JobSeekerAnswers.Add(testAnswer);
                    }
                    await _context.SaveChangesAsync();

                    // Calculate the score and assign badge based on answers
                    var correctAnswers = job.SkillTests.ToDictionary(q => q.Id, q => q.CorrectAnswer);
                    int totalQuestions = correctAnswers.Count;
                    int correctCount = SkillTestAnswers.Count(a => correctAnswers.ContainsKey(a.Key) && correctAnswers[a.Key] == a.Value);

                    int score = (int)((double)correctCount / totalQuestions * 100);
                    string badge = score >= 80 ? "Success" : (score >= 50 ? "Medium" : "Low");


                    myApplication.TestScore = score;
                    myApplication.PerformanceBadge = badge;
                    _context.Update(myApplication);
                    await _context.SaveChangesAsync();

                }

                if (CV != null)
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cvs");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var uniqueFileName = Guid.NewGuid() + Path.GetExtension(CV.FileName);
                    var filePath = Path.Combine(uploadPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await CV.CopyToAsync(stream);
                    }

                    myApplication.CVPath = "/uploads/cvs/" + uniqueFileName;
                    _context.Update(myApplication);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Application submitted successfully!";
                return RedirectToAction("Index", "Job");
            }

            return View(application);
        }
        [HttpGet]
        public IActionResult UploadCV(int applicationId)
        {
            var application = _context.Applications.FirstOrDefault(a => a.Id == applicationId);

            if (application == null || application.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }

            return View(new UploadCVViewModel { ApplicationId = applicationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCV(UploadCVViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var application = _context.Applications.FirstOrDefault(a => a.Id == model.ApplicationId);

            if (application == null || application.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cvs");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var uniqueFileName = Guid.NewGuid() + Path.GetExtension(model.CV.FileName);
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.CV.CopyToAsync(stream);
            }

            application.CVPath = "/uploads/cvs/" + uniqueFileName;
            _context.Update(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "CV uploaded successfully!";
            return RedirectToAction("Index", "Job");
        }
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Countries = _context.Countries.OrderBy(c => c.Name).ToList();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser model, IFormFile? profilePicture)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = _context.Countries.OrderBy(c => c.Name).ToList();
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;

            if (model.CountryId.HasValue)
            {
                var country = _context.Countries.FirstOrDefault(c => c.Id == model.CountryId.Value);
                if (country != null)
                {
                    user.CountryId = country.Id;
                    user.Country = country.Name;
                }
            }

            if (profilePicture != null && profilePicture.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(profilePicture.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                user.ProfilePicturePath = "/uploads/" + fileName;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Details", new { email = user.Email });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var jobseeker = _context.Jobseekers.Find(id);
            if (jobseeker != null)
            {
                _context.Jobseekers.Remove(jobseeker);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");

        }

        [Route("Jobseekers/Details/{email}")]
        public IActionResult Details(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserEmail = User.Identity?.Name;
            bool isOwnProfile = string.Equals(currentUserEmail, user.Email, StringComparison.OrdinalIgnoreCase);

            ViewBag.IsOwnProfile = isOwnProfile;

            return View(user);
        }
        public IActionResult Detailsaaa(int id)
        {
            var jobseeker = _context.Jobseekers.Find(id);
            if (jobseeker == null)
            {
                return NotFound();
            }
            return View(jobseeker);
        }
    }
}