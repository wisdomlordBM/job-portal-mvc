using Jobportalwebsite.Data;
using Jobportalwebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Jobportalwebsite.Services;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Jobportalwebsite.Controllers
{
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public JobController(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        private void AttachApplicantCounts(IEnumerable<Job> jobs)
        {
            var jobIds = jobs.Select(j => j.Id).ToList();
            var counts = _context.Applications
                .Where(a => jobIds.Contains(a.JobId))
                .GroupBy(a => a.JobId)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var job in jobs)
            {
                job.ApplicantCount = counts.TryGetValue(job.Id, out var count) ? count : 0;
            }
        }

        public IActionResult Index()
        {
            var listedJobs = _context.Jobs
                .Include(job => job.Company)
                .ThenInclude(company => company!.Country)
                .ThenInclude(country => country!.Currency)
                .Where(x => x.PostStatus == JobPostStatus.Posted)
                .OrderByDescending(y => y.DatePosted)
                .ToList();

            AttachApplicantCounts(listedJobs);

            var notifications = _context.Notifications
                .Where(n => n.UserId == User.Identity.Name)
                .OrderByDescending(n => n.Date)
                .Take(10)
                .ToList();

            ViewData["Notifications"] = notifications;

            return View(listedJobs);
        }

        // Admin approves a job
        public IActionResult ApproveJob(int id)
        {
            var job = _context.Jobs.FirstOrDefault(j => j.Id == id);
            if (job != null)
            {
                job.PostStatus = JobPostStatus.Posted;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult AllJobs()
        {
            var jobs = _context.Jobs
                .Include(job => job.Company)
                .ThenInclude(company => company!.Country)
                .ThenInclude(country => country!.Currency)
                .Where(j => j.PostStatus == JobPostStatus.Posted)
                .ToList();

            AttachApplicantCounts(jobs);

            return View(jobs);
        }

        // GET: Job/Create
        [HttpGet]
        public IActionResult Create()
        {
            var companyEmail = User.Identity?.Name;
            var company = _context.Companies.FirstOrDefault(c => c.Email == companyEmail);
            if (company == null)
            {
                return RedirectToAction("CompanyRegistration", "Company");
            }

            ViewBag.CompanyId = company.Id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJob(Job job, IFormFile? imageFile)
        {
            ValidateSalary(job);

            if (ModelState.IsValid)
            {
                var company = _context.Companies.FirstOrDefault(c => c.Email == User.Identity.Name);

                if (company == null)
                {
                    return RedirectToAction("CompanyRegistration", "Company");
                }

                job.CompanyId = company.Id;
                job.DatePosted = DateTime.UtcNow;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "jobs");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    job.ImageUrl = "/uploads/jobs/" + fileName;
                }

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                await _notificationService.NotifyAdminAsync(
                    $"A new job '{job.JobTitle}' has been posted by {company.Name}.",
                    jobId: job.Id,
                    companyId: company.Id);

                return RedirectToAction("Create", "JobSkillTest", new { jobId = job.Id });
            }

            return View(job);
        }

        public IActionResult ViewApplications(int jobId)
        {
            var applications = _context.Applications
                .Include(a => a.User)
                .Where(a => a.JobId == jobId)
                .Select(a => new
                {
                    a.User.FirstName,
                    a.User.LastName,
                    a.User.Email,
                    a.User.Address,
                    a.Contact,
                    a.Description,
                    a.DateApplied,
                    a.PerformanceBadge,
                    a.TestScore
                }).ToList();

            return View(applications);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var JobsFromDb = _context.Jobs.Find(id);

            if (JobsFromDb == null)
            {
                return NotFound();
            }

            return View(JobsFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Job obj, IFormFile? imageFile)
        {
            ValidateSalary(obj);

            if (ModelState.IsValid)
            {
                var jobFromDb = _context.Jobs.AsNoTracking().FirstOrDefault(j => j.Id == obj.Id);

                if (jobFromDb == null)
                    return NotFound();

                obj.CompanyId = jobFromDb.CompanyId;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "jobs");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    obj.ImageUrl = "/uploads/jobs/" + fileName;
                }
                else
                {
                    obj.ImageUrl = jobFromDb.ImageUrl;
                }

                _context.Jobs.Update(obj);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Company");
            }

            return View(obj);
        }

        private void ValidateSalary(Job job)
        {
            if (job.SalaryPeriod == SalaryPeriod.Negotiable)
            {
                job.Salary = null;
            }
            else if (!job.Salary.HasValue)
            {
                ModelState.AddModelError(nameof(job.Salary), "Salary amount is required unless salary is negotiable.");
            }
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var JobsFromDb = _context.Jobs.Find(id);
            if (JobsFromDb == null)
            {
                return NotFound();
            }

            return View(JobsFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _context.Jobs.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _context.Jobs.Remove(obj);
            _context.SaveChanges();
            return RedirectToAction("Index", "company");
        }

        [HttpPost]
        public IActionResult Index(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                return RedirectToAction("Index");
            }
            var filteredJob = _context.Jobs
                .Include(job => job.Company)
                .ThenInclude(company => company!.Country)
                .ThenInclude(country => country!.Currency)
                .AsEnumerable()
                .Where(p => p.Location.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || p.JobTitle.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                || p.EmploymentType.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                .ToList();

            AttachApplicantCounts(filteredJob);

            return View("Index", filteredJob);
        }

        public IActionResult Details(int id)
        {
            var job = _context.Jobs
                .Include(j => j.Company)
                .ThenInclude(company => company!.Country)
                .ThenInclude(country => country!.Currency)
                .FirstOrDefault(j => j.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            AttachApplicantCounts(new[] { job });

            return View(job);
        }
    }
}