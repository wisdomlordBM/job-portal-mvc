using Jobportalwebsite.Data;
using Jobportalwebsite.IHelper;
using Jobportalwebsite.Models;
using Jobportalwebsite.Viewmodel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Jobportalwebsite.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserHelper _userHelper;

        public AccountController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IUserHelper userHelper)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userHelper = userHelper;
        }




        // GET: Company Register
        [HttpGet]
        public IActionResult CompanyRegister()
        {
            LoadCountries();
            return View();
        }

        // POST: Company Register
        [HttpPost]
        public async Task<IActionResult> CompanyRegister(RegistrationViewModel model)
        {
            if (!model.CountryId.HasValue || !_context.Countries.Any(country => country.Id == model.CountryId.Value))
            {
                ModelState.AddModelError(nameof(model.CountryId), "Please select a valid country.");
                LoadCountries();
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "The password and confirm password do not match.");
                LoadCountries();
                return View(model);
            }

            var userEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userEmail != null)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                LoadCountries();
                return View(model);
            }

            var addUser = await _userHelper.CreateUserByAsync(model, "Company");
            if (addUser != null)
            {
                var company = new Company
                {
                    Email = model.Email,
                    CountryId = model.CountryId,
                    OnboardingStep = CompanyOnboardingStep.Details
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Company registered successfully.";
                return RedirectToAction("Login", "Account");
            }

            LoadCountries();
            return View(model);
        }

        // GET: Jobseeker Register
        [HttpGet]
        public IActionResult Register()
        {
            LoadCountries();
            return View();
        }

        // POST: Jobseeker Register
        [HttpPost]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {
            if (!model.CountryId.HasValue || !_context.Countries.Any(country => country.Id == model.CountryId.Value))
            {
                ModelState.AddModelError(nameof(model.CountryId), "Please select a valid country.");
                LoadCountries();
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                LoadCountries();
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "The password and confirm password do not match.");
                LoadCountries();
                return View(model);
            }

            var userEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userEmail != null)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                LoadCountries();
                return View(model);
            }

            var addUser = await _userHelper.CreateUserByAsync(model, "Jobseeker");
            if (addUser != null)
            {
                TempData["Message"] = "Jobseeker registered successfully.";
                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError(string.Empty, "An error occurred while creating the user.");
            LoadCountries();
            return View(model);
        }

        // ---------------- Standard Login & Logout ----------------

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Please register first.";
                return RedirectToAction("Register", "Account");
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                if (roles.Contains("Company"))
                {
                    var company = _context.Companies.FirstOrDefault(c => c.Email == model.Email);
                    return RedirectToCompanyStep(company);
                }
                if (roles.Contains("Jobseeker"))
                {
                    return RedirectToAction("Index", "Job");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            Response.Cookies.Delete(".AspNetCore.Identity.Application");
            return RedirectToAction("Login", "Account");
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string role, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            if (!string.IsNullOrEmpty(role))
            {
                properties.Items["role"] = role;
            }

            return Challenge(properties, provider);
        }

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            string role = null;
            if (info.AuthenticationProperties?.Items.ContainsKey("role") == true)
            {
                role = info.AuthenticationProperties.Items["role"];
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var user = await _userManager.FindByEmailAsync(email);
                return await RedirectByUserRole(user, returnUrl);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (email != null)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                        };

                        var result = await _userManager.CreateAsync(user);
                        if (result.Succeeded)
                        {
                            result = await _userManager.AddLoginAsync(user, info);
                            if (result.Succeeded)
                            {
                                if (!string.IsNullOrEmpty(role))
                                {
                                    await _userManager.AddToRoleAsync(user, role);
                                }
                                await _signInManager.SignInAsync(user, isPersistent: false);
                                return await RedirectByUserRole(user, returnUrl);
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(role) && !await _userManager.IsInRoleAsync(user, role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                        var result = await _userManager.AddLoginAsync(user, info);
                        if (result.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return await RedirectByUserRole(user, returnUrl);
                        }
                    }
                }

                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
            }
        }

        private void LoadCountries()
        {
            ViewBag.Countries = _context.Countries.OrderBy(country => country.Name).ToList();
        }

        private async Task<IActionResult> RedirectByUserRole(ApplicationUser user, string returnUrl)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            if (roles.Contains("Company"))
            {
                return RedirectToAction("Index", "Company");
            }
            if (roles.Contains("Jobseeker"))
            {
                return RedirectToAction("Index", "Job");
            }
            return LocalRedirect(returnUrl);
        }
        private IActionResult RedirectToCompanyStep(Company? company)
        {
            if (company == null)
            {
                // Safety net only — shouldn't happen since registration always creates the row now.
                return RedirectToAction("CompanyRegistration", "Company");
            }

            return company.OnboardingStep switch
            {
                CompanyOnboardingStep.Details => RedirectToAction("OnboardingDetails", "Company", new { id = company.Id }),
                CompanyOnboardingStep.Branding => RedirectToAction("OnboardingBranding", "Company", new { id = company.Id }),
                CompanyOnboardingStep.OfficeInfo => RedirectToAction("OnboardingOffice", "Company", new { id = company.Id }),
                _ => RedirectToAction("Index", "Company")
            };
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var model = new ChangePasswordViewModel
            {
                HasExistingPassword = await _userManager.HasPasswordAsync(user)
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            model.HasExistingPassword = await _userManager.HasPasswordAsync(user);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            IdentityResult result;

            if (model.HasExistingPassword)
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is required.");
                    return View(model);
                }
                result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            }
            else
            {
                // Google/Facebook/Microsoft-only account setting a password for the first time
                result = await _userManager.AddPasswordAsync(user, model.NewPassword);
            }

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Message"] = "Password updated successfully.";
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}