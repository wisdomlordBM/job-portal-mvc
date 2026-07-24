using Jobportalwebsite.Data;
using Jobportalwebsite.Models;
using Jobportalwebsite.Services;
using Jobportalwebsite.IHelper;
using Jobportalwebsite.Helper;
using Jobportalwebsite.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
// Configure logging for SignalR
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});
// Register SignalR services
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment(); // Detailed errors in development
});
// Register custom user ID provider for SignalR
builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
// Optional: Add CORS if the chat client is hosted on a different domain
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()
               .WithExposedHeaders("Content-Disposition");
    });
});
ConfigureServices(builder.Services, builder.Configuration);
var app = builder.Build();
// Apply migrations and seed roles asynchronously during app startup
await ApplyMigrationsAndSeedRolesAsync(app);

ConfigureMiddleware(app);
// Map the SignalR hub for chat functionality
app.MapHub<NotificationHub>("/notificationHub");

app.MapHub<ChatHub>("/chatHub");

app.Run();
void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddControllersWithViews();
    services.AddRazorPages().AddRazorRuntimeCompilation();
    // Register application services
    services.AddScoped<IUserHelper, UserHelper>();
    services.AddScoped<NotificationService>();
    // Configure DbContext to use SQL Server
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    // Configure Identity services
    services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        // Identity password policy
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 1;
        options.Password.RequiredUniqueChars = 1;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // Register external authentication providers - only when real credentials are configured,
    // so a missing/placeholder provider doesn't crash the whole app on startup validation.
    var authBuilder = services.AddAuthentication();

    var googleClientId = configuration["Authentication:Google:ClientId"];
    var googleClientSecret = configuration["Authentication:Google:ClientSecret"];
    if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
    {
        authBuilder.AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });
    }

    var facebookAppId = configuration["Authentication:Facebook:AppId"];
    var facebookAppSecret = configuration["Authentication:Facebook:AppSecret"];
    if (!string.IsNullOrWhiteSpace(facebookAppId)
        && !string.IsNullOrWhiteSpace(facebookAppSecret)
        && facebookAppId != "YOUR_FACEBOOK_APP_ID")
    {
        authBuilder.AddFacebook(options =>
        {
            options.AppId = facebookAppId;
            options.AppSecret = facebookAppSecret;
        });
    }

    var microsoftClientId = configuration["Authentication:Microsoft:ClientId"];
    var microsoftClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
    if (!string.IsNullOrWhiteSpace(microsoftClientId)
        && !string.IsNullOrWhiteSpace(microsoftClientSecret)
        && microsoftClientId != "YOUR_MICROSOFT_CLIENT_ID")
    {
        authBuilder.AddMicrosoftAccount(options =>
        {
            options.ClientId = microsoftClientId;
            options.ClientSecret = microsoftClientSecret;
        });
    }
}
async Task ApplyMigrationsAndSeedRolesAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    // Get services for the context, role manager, user manager, and configuration
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var configuration = services.GetRequiredService<IConfiguration>();

    // Apply any pending migrations and seed roles
    await context.Database.MigrateAsync();
    await CountryCurrencySeeder.SeedAsync(context);
    await SeedRolesAsync(roleManager);
    await AdminSeeder.SeedAsync(userManager, roleManager, configuration);
}
void ConfigureMiddleware(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    // Apply CORS policy before authentication if needed
    app.UseCors("AllowAll"); // Remove this line if CORS is not required
    app.UseAuthentication();
    app.UseAuthorization();
    // Map default controller route
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
}
static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roleNames = { "Admin", "Company", "Jobseeker" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}