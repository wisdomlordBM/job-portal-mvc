using Jobportalwebsite.Data;
using Jobportalwebsite.Models;
using Jobportalwebsite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Jobportalwebsite.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager; // Inject UserManager

        public NotificationController(ApplicationDbContext context, NotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _notificationService = notificationService;  // Initialize NotificationService
            _userManager = userManager;  // Initialize UserManager
        }

        // Action to create a notification for a new job posting
        public void NotifyNewJob(Job job)
        {
            var notification = new Notification
            {
                Message = "A new job has been posted.",
                IsRead = false,
                Date = DateTime.UtcNow,
                Type = NotificationType.NewJob,
                JobId = job.Id,
                JobTitle = job.JobTitle,
                CompanyName = job.Company.Name
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }

        // Action to create a notification for a new company registration
        public void NotifyNewCompany(Company company)
        {
            var notification = new Notification
            {
                Message = "A new company has registered.",
                IsRead = false,
                Date = DateTime.UtcNow,
                Type = NotificationType.NewCompany,
                CompanyId = company.Id,
                CompanyName = company.Name,
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }

        // Action to mark a notification as read
        [HttpPost]
        public IActionResult MarkAsRead([FromBody] NotificationReadRequest request)
        {
            var notification = _context.Notifications.FirstOrDefault(n => n.Id == request.Id);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        public class NotificationReadRequest
        {
            public int Id { get; set; }
        }


        // Returns only the current user's own unread alerts (works for jobseeker, company, or anyone else who receives a UserAlert)
         [HttpGet]
         [Authorize]
         public IActionResult GetMyNotifications()
        {
             var userId = _userManager.GetUserId(User);
             var notifications = _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead && n.Type == NotificationType.UserAlert)
            .OrderByDescending(n => n.Date)
            .Take(10)
            .Select(n => new { n.Id, n.Message, n.Date })
            .ToList();

             return Json(notifications);
         }

           [HttpGet]
             [Authorize]
              public IActionResult GetMyNotificationCount()
                 {
                    var userId = _userManager.GetUserId(User);
                    var count = _context.Notifications
                        .Count(n => n.UserId == userId && !n.IsRead && n.Type == NotificationType.UserAlert);

                    return Json(count);
                }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MarkAllMyNotificationsAsRead()
        {
                var userId = _userManager.GetUserId(User);
            var notifications = _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead && n.Type == NotificationType.UserAlert)
                .ToList();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Ok();
         }



}


}
