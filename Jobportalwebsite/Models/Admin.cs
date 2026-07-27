using Jobportalwebsite.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Jobportalwebsite.Models
{
    public class Admin : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; } // You could specify roles (e.g., SuperAdmin, Manager)

        public ICollection<Notification>? Notifications { get; set; }

        public void NotifyAdmin(string message)
        {
            Notifications.Add(new Notification { Message = message, Date = DateTime.UtcNow });
        }
    }
}

