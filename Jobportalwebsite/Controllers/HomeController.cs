using Jobportalwebsite.Data;
using Jobportalwebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Jobportalwebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;  // Add this line to define _context

        // Modify the constructor to inject ApplicationDbContext
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;  // Assign the injected context to _context
        }

        public IActionResult Index()
        {
            // Fetch jobs where PostStatus is "Posted" and sort them
            var objJobList = _context.Jobs
                .Include(job => job.Company)
                .ThenInclude(company => company!.Country)
                .ThenInclude(country => country!.Currency)
                .Where(x => x.PostStatus == JobPostStatus.Posted)
                .OrderByDescending(y => y.DatePosted)
                .Take(12) // Take top 5 jobs or change as necessary
                .ToList();

            return View(objJobList); // Passing the job list to the view
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

