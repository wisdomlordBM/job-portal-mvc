using Jobportalwebsite.Models;
using Microsoft.AspNetCore.Identity;

namespace Jobportalwebsite.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            var email = configuration["DefaultAdmin:Email"];
            var password = configuration["DefaultAdmin:Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return; // Nothing configured — skip silently.
            }

            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                return; // Admin already exists — don't recreate or reset password.
            }

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Role = "Admin",
                DateCreated = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}