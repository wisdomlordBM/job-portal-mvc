using Jobportalwebsite.Data;
using Jobportalwebsite.IHelper;
using Jobportalwebsite.Models;
using Jobportalwebsite.Viewmodel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Jobportalwebsite.Helper
{
    public class UserHelper : IUserHelper
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserHelper(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<ApplicationUser> CreateUserByAsync(RegistrationViewModel model, string role)
        {
            if (model == null)
            {
                return null;
            }

            // Admins don't need a country; Company/Jobseeker do.
            Country? country = null;
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                country = await GetCountryAsync(model.CountryId);
                if (country == null)
                {
                    return null;
                }
            }

            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                DateCreated = DateTime.Now,
                PhoneNumber = model.PhoneNumber,
                State = model.State,
                Country = country?.Name,
                CountryId = country?.Id,
                Email = model.Email,
                UserName = model.Email,
                Role = role
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
                await _userManager.AddToRoleAsync(user, role);
                return user;
            }
            return null;
        }

        private Task<Country?> GetCountryAsync(int? countryId)
        {
            return countryId.HasValue
                ? _context.Countries.SingleOrDefaultAsync(country => country.Id == countryId.Value)
                : Task.FromResult<Country?>(null);
        }

        public async Task<List<ApplicationUser>> GetAllOtherUsersAsync(string currentUser)
        {
            var users = await _userManager.Users
                .Where(u => u.UserName != currentUser)
                .ToListAsync();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.Role = roles.FirstOrDefault();

                if (user.Role == "Company")
                {
                    var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == user.Email);
                    if (company != null)
                    {
                        user.ProfilePicturePath = company.ProfilePicturePath;
                        user.FirstName = company.Name;
                    }
                }
            }

            return users;
        }
    }
}