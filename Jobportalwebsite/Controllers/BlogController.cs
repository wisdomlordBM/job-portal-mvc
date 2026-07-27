

using Jobportalwebsite.Data;
using Jobportalwebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Jobportalwebsite.Viewmodel;
using Jobportalwebsite.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Jobportalwebsite.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public BlogController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var blogs = await _context.Blogs
                .Where(b => !b.Deleted)

                 .Include(b => b.Company) 
                 // Include the related Company data

                .OrderByDescending(b => b.DateCreated)
                .ToListAsync();
            return View(blogs);
        }


       
        public async Task<IActionResult> Detailsh(int id)
        {
            var blog = await _context.Blogs
              .Include(b => b.Comments.OrderByDescending(c => c.DateCommented)) // Sort Comments
                  .ThenInclude(c => c.Replies.OrderByDescending(r => r.DateReplied)) // Sort Replies
              .FirstOrDefaultAsync(b => b.Id == id && !b.Deleted);


            if (blog == null)
            {
                return NotFound();
            }
            // Sort comments by date (latest first)
            blog.Comments = blog.Comments
                .OrderByDescending(c => c.DateCommented)
                .ToList();
            // Fetch profile pictures dynamically for all comments and replies
            foreach (var comment in blog.Comments)
            {
                if (!comment.IsAnonymous)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == comment.CommentedBy);
                    var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == comment.CommentedBy);

                    comment.ProfilePicturePath = company?.ProfilePicturePath ?? user?.ProfilePicturePath ?? "/images/default-profile.png";
                }

                // Fetch profile pictures for replies
                if (comment.Replies != null)
                {
                    foreach (var reply in comment.Replies)
                    {
                        if (!reply.IsAnonymous)
                        {
                            var replyUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == reply.RepliedBy);
                            var replyCompany = await _context.Companies.FirstOrDefaultAsync(c => c.Email == reply.RepliedBy);

                            reply.ProfilePicturePath = replyCompany?.ProfilePicturePath ?? replyUser?.ProfilePicturePath ?? "/images/default-profile.png";
                        }
                    }
                }
            }

            return View(blog);
        }

        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.Blogs
                .Include(b => b.Comments.OrderByDescending(c => c.DateCommented)) // Sort Comments
                    .ThenInclude(c => c.Replies.OrderByDescending(r => r.DateReplied)) // Sort Replies
                .FirstOrDefaultAsync(b => b.Id == id && !b.Deleted);

            if (blog == null)
            {
                return NotFound();
            }

            // Sort comments by date (latest first)
            blog.Comments = blog.Comments
                .OrderByDescending(c => c.DateCommented)
                .ToList();

            // Fetch profile pictures and roles dynamically for all comments and replies
            foreach (var comment in blog.Comments)
            {
                if (!comment.IsAnonymous)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == comment.CommentedBy);
                    var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == comment.CommentedBy);

                    comment.ProfilePicturePath = company?.ProfilePicturePath ?? user?.ProfilePicturePath ?? "/images/default-profile.png";

                    if (user != null)
                    {
                        // Get the roles for the user
                        var roles = await _userManager.GetRolesAsync(user);
                        comment.UserRole = roles.FirstOrDefault(); // Assuming a single role per user
                    }
                    else if (company != null)
                    {
                        comment.UserRole = "Company"; // Set role explicitly for companies
                    }
                }

                // Fetch profile pictures and roles for replies
                if (comment.Replies != null)
                {
                    foreach (var reply in comment.Replies)
                    {
                        if (!reply.IsAnonymous)
                        {
                            var replyUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == reply.RepliedBy);
                            var replyCompany = await _context.Companies.FirstOrDefaultAsync(c => c.Email == reply.RepliedBy);

                            reply.ProfilePicturePath = replyCompany?.ProfilePicturePath ?? replyUser?.ProfilePicturePath ?? "/images/default-profile.png";

                            if (replyUser != null)
                            {
                                // Get the roles for the reply user
                                var roles = await _userManager.GetRolesAsync(replyUser);
                                reply.UserRole = roles.FirstOrDefault(); // Assuming a single role per user
                            }
                            else if (replyCompany != null)
                            {
                                reply.UserRole = "Company"; // Set role explicitly for companies
                            }
                        }
                    }
                }
            }

            return View(blog);
        }
        private async Task<string> GetProfilePicturePathAsync(string username, bool isAnonymous)
        {
            if (isAnonymous)
            {
                return "/images/anonymous-icon.png";
            }

            if (string.IsNullOrEmpty(username))
            {
                return "/images/default-profile.png";
            }

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == username);
            if (company != null)
            {
                return company.ProfilePicturePath ?? "/images/default-profile.png";
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            return user?.ProfilePicturePath ?? "/images/default-profile.png";
        }

        [Authorize(Roles = "Company")]
        public IActionResult Create()
        {
            return View();
        }

 

        [HttpPost]
        [Authorize(Roles = "Company")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Blog blog, List<IFormFile> pictures)
        {
            if (ModelState.IsValid)
            {
                var picturePaths = new List<string>();

                if (pictures != null && pictures.Any())
                {
                    var uploadFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "Blog");

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    foreach (var picture in pictures)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(picture.FileName);
                        var extension = Path.GetExtension(picture.FileName);
                        var uniqueFileName = fileName + "_" + System.Guid.NewGuid().ToString() + extension;

                        var filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await picture.CopyToAsync(stream);
                        }

                        picturePaths.Add("/uploads/Blog/" + uniqueFileName);
                    }
                }

                blog.PicturePaths = picturePaths; 
                blog.DateCreated = DateTime.UtcNow;
                blog.CreatedBy = User.Identity.Name;
                _context.Blogs.Add(blog);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(blog);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int blogId, string commentText, bool isAnonymous, int? parentCommentId = null)
        {
            var blog = await _context.Blogs.FindAsync(blogId);

            if (blog == null)
            {
                return NotFound();
            }

            string profilePicturePath;

            if (isAnonymous)
            {
                profilePicturePath = "/images/anonymous.png";
            }
            else
            {
                // Check if the current user is a company
                var currentUser = User.Identity.Name;
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == currentUser);
                if (company != null)
                {
                    profilePicturePath = company.ProfilePicturePath ?? "/images/default-profile.png";
                }
                else
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == currentUser);
                    profilePicturePath = user?.ProfilePicturePath ?? "/images/default-profile.png";
                }
            }

            var comment = new Comment
            {
                CommentText = commentText,
                DateCommented = DateTime.UtcNow,
                IsAnonymous = isAnonymous,
                BlogId = blogId,
                CommentedBy = isAnonymous ? null : User.Identity.Name,
                ProfilePicturePath = profilePicturePath,
                //ParentCommentId = parentCommentId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = blogId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReply(int commentId, string replyText, bool isAnonymous)
        {
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
            {
                return NotFound();
            }

            string profilePicturePath = isAnonymous ? "/images/anonymous.png" : await GetUserProfilePicture(User.Identity.Name);

            var reply = new Reply
            {
                ReplyText = replyText,
                DateReplied = DateTime.UtcNow,
                IsAnonymous = isAnonymous,
                RepliedBy = isAnonymous ? null : User.Identity.Name,
                ProfilePicturePath = profilePicturePath,
                CommentId = commentId
            };

            _context.Replies.Add(reply);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = comment.BlogId });
        }
        private async Task<string> GetUserProfilePicture(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            return user?.ProfilePicturePath ?? "/images/default-profile.png";
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.Include(b => b.Comments)
                                           .FirstOrDefaultAsync(m => m.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            if (blog.CreatedBy != User.Identity.Name)
            {
                return Forbid(); // Prevent unauthorized deletion
            }

            // Delete the blog and its related comments
            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);

            if (blog == null)
            {
                return NotFound();
            }

           
            if (blog.CreatedBy != User.Identity.Name)
            {
                return Forbid(); 
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
