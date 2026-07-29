using Jobportalwebsite.Data;
using Jobportalwebsite.Models;
using Jobportalwebsite.Services;  // Make sure NotificationService is correctly imported
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Jobportalwebsite.ViewModel;
using Microsoft.AspNetCore.Identity;
using Jobportalwebsite.Viewmodel;

namespace Jobportalwebsite.Controllers
{
    //[Authorize]
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FileStorageService _fileStorageService; // Inject UserManager

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

        // Constructor to inject ApplicationDbContext and NotificationService
        public CompanyController(
             ApplicationDbContext context,
             NotificationService notificationService,
             UserManager<ApplicationUser> userManager,
             FileStorageService fileStorageService)
        {
            _context = context;
            _notificationService = notificationService;
            _userManager = userManager;
            _fileStorageService = fileStorageService;
        }

        // GET: Company/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company company)
        {
            if (ModelState.IsValid)
            {
                company.Email = User.Identity.Name;
                var user = await _userManager.GetUserAsync(User);
                company.CountryId = user?.CountryId;

                if (_context.Companies.Any(c => c.Email == company.Email || c.Name == company.Name))
                {
                    ModelState.AddModelError("", "Company with the same email or name already exists.");
                    return View(company);
                }

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                await _notificationService.NotifyAdminAsync(
                    $"A new company '{company.Name}' has registered.",
                    companyId: company.Id
                );

                return RedirectToAction("Index");
            }

            return View(company);
        }


        public IActionResult ViewApplications(int companyId)
        {
            var applications = _context.Applications
                .Where(a => a.Job != null && a.Job.Company != null && a.Job.Company.Id == companyId)
                .Include(a => a.Job)
                .ThenInclude(j => j.Company)
                .Include(a => a.User)  // Include the user profile
                .Select(a => new ApplicationViewModel
                {
                    Id = a.Id,
                    JobTitle = a.Job.JobTitle,
                    ImageUrl = a.Job.ImageUrl,
                    CompanyId = a.Job.Company.Id,
                    Company = a.Job.Company,
                    EmploymentType = a.Job.EmploymentType,
                    Location = a.Job.Location,
                    Salary = a.Job.Salary,
                    SalaryPeriod = a.Job.SalaryPeriod,
                    CurrencySymbol = a.Job.Company.Country != null ? a.Job.Company.Country.Currency.Symbol : null,
                    UserId = a.UserId,
                    Email = a.User.Email ?? "N/A",
                    FirstName = a.User.FirstName ?? "N/A",
                    LastName = a.User.LastName ?? "N/A",
                    PhoneNumber = a.User.PhoneNumber ?? "N/A",
                    EducationLevel = a.EducationLevel,
                    CVPath = a.CVPath,
                    Contact = a.Contact,
                    Description = a.Description,
                    DateApplied = a.DateApplied,
                    City = a.City,

                    // Ensure TestScore and PerformanceBadge are included
                    TestScore = a.TestScore ?? 0,
                    PerformanceBadge = a.PerformanceBadge ?? "No Test Taken",

                    // Include Profile Picture
                    ProfilePicturePath = a.User.ProfilePicturePath ?? "/uploads/default-profile.png"
                })
                .ToList();

            return View(applications);
        }





        [HttpPost]
        public async Task<IActionResult> Check(int applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.User)
                .Include(j => j.Job)
                .ThenInclude(c => c.Company)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application != null)
            {
                var userId = application.UserId;
                var companyName = application.Job?.Company?.Name ?? "the company";
                var companyId = application.Job?.Company?.Id;

                var message = $"Your application is under review by {companyName}.";

                await _notificationService.NotifyUserAsync(userId, message);

                TempData["Message"] = "User has been notified about the review status.";

                if (companyId.HasValue)
                {
                    return RedirectToAction("ViewApplications", "Company", new { companyId = companyId.Value });
                }
            }

            return RedirectToAction("ViewApplications", "Company");
        }


        [HttpPost]
        public async Task<IActionResult> Accept(int applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.User)
                .Include(j => j.Job)
                .ThenInclude(c => c.Company)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application != null)
            {
                var userId = application.UserId;
                var companyId = application.Job?.Company?.Id;
                var message = $"Congratulations! {application.Job?.Company?.Name ?? "The company"} has accepted your application. They will contact you soon.";

                // Notify the user (job seeker) via SignalR
                await _notificationService.NotifyUserAsync(userId, message);

                TempData["Message"] = "User has been notified of acceptance.";

                if (companyId.HasValue)
                {
                    return RedirectToAction("ViewApplications", "Company", new { companyId = companyId.Value });
                }
            }

            return RedirectToAction("ViewApplications", "Company");
        }

        [HttpPost]
        public async Task<IActionResult> Decline(int applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.User)
                .Include(a => a.JobSeekerAnswers)
                .Include(j => j.Job)
                .ThenInclude(c => c.Company)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application != null)
            {
                var userId = application.UserId;
                var companyId = application.Job?.Company?.Id;
                var message = $"We regret to inform you that your application for {application.Job?.JobTitle ?? "the job"} has been declined.";

                await _notificationService.NotifyUserAsync(userId, message);

                if (application.JobSeekerAnswers != null && application.JobSeekerAnswers.Any())
                {
                    _context.JobSeekerAnswers.RemoveRange(application.JobSeekerAnswers);
                }

                _context.Applications.Remove(application); 
                await _context.SaveChangesAsync();

                TempData["Message"] = "User has been notified of the decline.";

                if (companyId.HasValue)
                {
                    return RedirectToAction("ViewApplications", "Company", new { companyId = companyId.Value });
                }
            }

            return RedirectToAction("ViewApplications", "Company");
        }


        [HttpGet]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userId = _userManager.GetUserId(User);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && n.Type == NotificationType.UserAlert)
                .OrderByDescending(n => n.Date)
                .Take(5)
                .Select(n => new { n.Message, n.Date })
                .ToListAsync();

            ViewData["Notifications"] = notifications;
            ViewData["UnreadCount"] = notifications.Count;


            return Json(notifications);
        }
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _userManager.GetUserId(User);

            var notifications = _context.Notifications.Where(n => !n.IsRead && n.Type == NotificationType.UserAlert).ToList();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Job");
        }
        [HttpGet]
        public IActionResult GetUserNotificationCount()
        {
            var unreadCount = _context.Notifications.Count(n => !n.IsRead && n.Type == NotificationType.UserAlert);
            return Json(unreadCount);
        }


        //GET: Company/Index
        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.Identity.Name;

            var company = _context.Companies.FirstOrDefault(c => c.Email == userEmail);
            if (company == null || company.OnboardingStep != CompanyOnboardingStep.Completed)
            {
                return RedirectToOnboardingStep(company);
            }

            // You can decide to leave out the jobs in the index view if you only want them on a separate page
            var viewModel = new CompanyDashboardViewModel
            {
                CompanyId = company.Id,
                Name = company.Name,
                Location = company.Location,
                Industry = company.Industry,
                WebsiteUrl = company.WebsiteUrl,
                Phone = company.Phone,
                ProfilePicturePath = company.ProfilePicturePath,
                CoverBannerPath = company.CoverBannerPath,
                Address = company.Address,
                City = company.City,
                State = company.State,
                PostalCode = company.PostalCode,
                CountryName = _context.Companies
                .Where(c => c.Id == company.Id)
                .Select(c => c.Country != null ? c.Country.Name : null)
                .FirstOrDefault()
            };

            return View(viewModel);
        }

        //GET: Company/Jobs
        public IActionResult Jobs(int companyId)
        {
            var company = _context.Companies
                .Include(c => c.Country)
                .ThenInclude(country => country!.Currency)
                .FirstOrDefault(c => c.Id == companyId);

            if (company == null)
            {
                return NotFound();
            }

            var jobs = _context.Jobs
                .Where(j => j.CompanyId == companyId)
                .Select(j => new JobViewModel
                {
                    Id = j.Id,
                    JobTitle = j.JobTitle,
                    Description = j.Description,
                    Location = j.Location,
                    EmploymentType = j.EmploymentType,
                    Salary = j.Salary,
                    SalaryPeriod = j.SalaryPeriod,
                    CurrencySymbol = company.Country == null ? null : company.Country.Currency.Symbol,
                    ImageUrl = j.ImageUrl,
                    JobPostStatus = j.PostStatus
                })
                .ToList();

            var jobsViewModel = new CompanyDashboardViewModel
            {
                CompanyId = companyId,
                Name = company.Name,
                Jobs = jobs
            };

            return View(jobsViewModel);
        }

        private IActionResult RedirectToOnboardingStep(Company? company)
        {
            if (company == null)
            {
                return RedirectToAction("CompanyRegistration");
            }

            return company.OnboardingStep switch
            {
                CompanyOnboardingStep.Details => RedirectToAction(nameof(OnboardingDetails), new { id = company.Id }),
                CompanyOnboardingStep.Branding => RedirectToAction(nameof(OnboardingBranding), new { id = company.Id }),
                CompanyOnboardingStep.OfficeInfo => RedirectToAction(nameof(OnboardingOffice), new { id = company.Id }),
                _ => RedirectToAction(nameof(Index))
            };
        }

        // GET: Company/OnboardingDetails/5
        [HttpGet]
        public IActionResult OnboardingDetails(int id)
        {
            var company = _context.Companies.Find(id);
            if (company == null) return NotFound();

            return View(company);
        }

        // POST: Company/OnboardingDetails/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OnboardingDetails(int id, Company model)
        {
            var company = _context.Companies.Find(id);
            if (company == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            company.Name = model.Name;
            company.Industry = model.Industry;
            company.Description = model.Description;
            company.Phone = model.Phone;
            company.WebsiteUrl = model.WebsiteUrl;

            if (company.OnboardingStep < CompanyOnboardingStep.Branding)
            {
                company.OnboardingStep = CompanyOnboardingStep.Branding;
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(OnboardingBranding), new { id = company.Id });
        }

        // GET: Company/OnboardingBranding/5
        [HttpGet]
        public IActionResult OnboardingBranding(int id)
        {
            var company = _context.Companies.Find(id);
            if (company == null) return NotFound();

            return View(company);
        }

        // POST: Company/OnboardingBranding/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnboardingBranding(int id, IFormFile? logoFile, IFormFile? coverBannerFile)
        {
            var company = _context.Companies.Find(id);
            if (company == null) return NotFound();

            bool hasExistingLogo = !string.IsNullOrEmpty(company.ProfilePicturePath);
            if ((logoFile == null || logoFile.Length == 0) && !hasExistingLogo)
            {
                ModelState.AddModelError("logoFile", "Please upload a company logo to continue.");
                return View(company);
            }

            if (logoFile != null && logoFile.Length > 0)
            {
                var (isValid, error) = ValidateImage(logoFile);
                if (!isValid)
                {
                    ModelState.AddModelError("logoFile", error!);
                    return View(company);
                }
                company.ProfilePicturePath = await SaveCompanyImageAsync(logoFile, "logos");
            }

            if (coverBannerFile != null && coverBannerFile.Length > 0)
            {
                var (isValid, error) = ValidateImage(coverBannerFile);
                if (!isValid)
                {
                    ModelState.AddModelError("coverBannerFile", error!);
                    return View(company);
                }
                company.CoverBannerPath = await SaveCompanyImageAsync(coverBannerFile, "banners");
            }

            if (company.OnboardingStep < CompanyOnboardingStep.OfficeInfo)
            {
                company.OnboardingStep = CompanyOnboardingStep.OfficeInfo;
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(OnboardingOffice), new { id = company.Id });
        }

        // GET: Company/OnboardingOffice/5
        [HttpGet]
        public IActionResult OnboardingOffice(int id)
        {
            var company = _context.Companies.Find(id);
            if (company == null) return NotFound();

            return View(company);
        }

        // POST: Company/OnboardingOffice/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OnboardingOffice(int id, Company model)
        {
            var company = _context.Companies.Find(id);
            if (company == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            company.Address = model.Address;
            company.City = model.City;
            company.State = model.State;
            company.PostalCode = model.PostalCode;
            company.OnboardingStep = CompanyOnboardingStep.Completed;

            _context.SaveChanges();

            TempData["Message"] = "Company profile completed! Welcome to your dashboard.";
            return RedirectToAction(nameof(Index));
        }

        private (bool IsValid, string? Error) ValidateImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                return (false, "Only JPG, JPEG, PNG, and WEBP images are allowed.");
            }
            if (file.Length > MaxImageSizeBytes)
            {
                return (false, "Image must be smaller than 5 MB.");
            }
            return (true, null);
        }

        private async Task<string> SaveCompanyImageAsync(IFormFile file, string subfolder)
        {
            return await _fileStorageService.UploadImageAsync(file, $"companies/{subfolder}");
        }

        // GET: Company/Edit/5
        public IActionResult Edit(int id)
        {
            var company = _context.Companies.Find(id);  // Find the company by ID
            if (company == null)
            {
                return NotFound();  // Return a 404 if the company is not found
            }
            return View(company);  // Return the company model to the Edit view
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Company company, IFormFile? profilePicture, IFormFile? coverBannerFile)
        {
            if (id != company.Id)
            {
                return NotFound(); // Return a 404 if the company ID does not match
            }

            if (!ModelState.IsValid)
            {
                return View(company);
            }

            var existingCompany = _context.Companies.Find(id);
            if (existingCompany == null)
            {
                return NotFound(); // Return a 404 if the company is not found
            }

            // Update logo if a new file is uploaded
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var (isValid, error) = ValidateImage(profilePicture);
                if (!isValid)
                {
                    ModelState.AddModelError("profilePicture", error!);
                    return View(company);
                }
                existingCompany.ProfilePicturePath = await SaveCompanyImageAsync(profilePicture, "logos");
            }

            // Update cover banner if a new file is uploaded
            if (coverBannerFile != null && coverBannerFile.Length > 0)
            {
                var (isValid, error) = ValidateImage(coverBannerFile);
                if (!isValid)
                {
                    ModelState.AddModelError("coverBannerFile", error!);
                    return View(company);
                }
                existingCompany.CoverBannerPath = await SaveCompanyImageAsync(coverBannerFile, "banners");
            }

            // Update other properties
            existingCompany.Name = company.Name;
            existingCompany.Description = company.Description;
            existingCompany.Industry = company.Industry;
            existingCompany.WebsiteUrl = company.WebsiteUrl;
            existingCompany.Phone = company.Phone;
            existingCompany.Address = company.Address;
            existingCompany.City = company.City;
            existingCompany.State = company.State;
            existingCompany.PostalCode = company.PostalCode;

            _context.SaveChanges();

            return RedirectToAction("Index", "Company"); // Redirect to the Index page if successful
        }


        // GET: Company/Delete/5
        public IActionResult Delete(int id)
        {
            var company = _context.Companies.Find(id);  // Find the company by ID
            if (company == null)
            {
                return NotFound();  // Return a 404 if the company is not found
            }
            return View(company);  // Return the company model to the Delete view
        }

        // POST: Company/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var company = _context.Companies.Find(id);  // Find the company by ID
            if (company != null)
            {
                _context.Companies.Remove(company);  // Remove the company from the database
                _context.SaveChanges();  // Save changes to the database
            }

            return RedirectToAction("Index");  // Redirect to the Index page after deletion
        }

        [Route("Company/Details/{email}")]
        public IActionResult Details(string email)
        {
            var company = _context.Companies.FirstOrDefault(c => c.Email == email);
            if (company == null)
            {
                return NotFound();
            }

            var currentUserEmail = User.Identity?.Name;
            bool isCompanyOwner = string.Equals(currentUserEmail, company.Email, StringComparison.OrdinalIgnoreCase);

            // Pass ownership status to the view
            ViewBag.IsCompanyOwner = isCompanyOwner;

            return View(company);
        }
        [HttpGet("Company/DetailsById/{id}")]
        public IActionResult DetailsById(int id)
        {
            var company = _context.Companies.FirstOrDefault(c => c.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            var currentUserEmail = User.Identity?.Name;
            bool isCompanyOwner = string.Equals(currentUserEmail, company.Email, StringComparison.OrdinalIgnoreCase);
            ViewBag.IsCompanyOwner = isCompanyOwner;


            return View("Details", company);
        }



        public IActionResult ViewAnswers(int applicationId)
        {
            var application = _context.Applications.FirstOrDefault(a => a.Id == applicationId);
            if (application == null)
            {
                return NotFound();
            }


            var testQuestions = _context.JobSkillTests
                                .Where(q => q.JobId == application.JobId)
                                .ToList();

            var userAnswers = _context.JobSeekerAnswers
                                .Where(a => a.ApplicationId == applicationId)
                                .ToList();

            var answers = (from question in testQuestions
                           join answer in userAnswers
                                on question.Id equals answer.JobSkillTestId into answerGroup
                           from userAnswer in answerGroup.DefaultIfEmpty()
                           select new AnswerViewModel
                           {
                               QuestionText = question.Question,
                               CorrectAnswer = question.CorrectAnswer,
                               UserSelectedAnswer = userAnswer != null ? userAnswer.SelectedAnswer : null
                           }).ToList();

            return View(answers);
        }




        public IActionResult CompanyRegistration()
        {
            return View();
        }
    }
}