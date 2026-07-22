


using Jobportalwebsite.Data;
using Jobportalwebsite.Models;
using Jobportalwebsite.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Jobportalwebsite.Hubs;

public class NotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext; // Inject SignalR Hub context

    public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext; 
    }

    public async Task NotifyAdminAsync(string message, int? companyId = null, int? jobId = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("Notification message is empty. No notification will be created.");
            return;
        }

        var notification = new Notification
        {
            Message = message,
            Date = DateTime.UtcNow,
            CompanyId = companyId,
            JobId = jobId,
            Type = jobId.HasValue ? NotificationType.NewJob : NotificationType.NewCompany,
            IsRead = false
        };

        try
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Notification created: {message}");
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving notification");
            throw;
        }
    }
    public async Task NotifyUserAsync(string userId, string message)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(message))
        {
            _logger.LogWarning("User ID or message is empty. Notification not sent.");
            return;
        }

        // Send real-time notification to the specific user
        await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);

        // Save the notification to the database
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            Date = DateTime.UtcNow,
            IsRead = false,
            Type = NotificationType.UserAlert // Categorize it as a user alert
        };

        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }


}
