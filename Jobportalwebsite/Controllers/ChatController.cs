
using Jobportalwebsite.Data;
using Jobportalwebsite.IHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jobportalwebsite.ViewModel; // Add this

namespace Jobportalwebsite.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ApplicationDbContext _context;

        public ChatController(IUserHelper userHelper, ApplicationDbContext context)
        {
            _userHelper = userHelper;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var currentUser = User.Identity?.Name;

            var users = await _userHelper.GetAllOtherUsersAsync(currentUser);

            var unreadCounts = await _context.ChatMessages
                .Where(m => m.ReceiverUserName == currentUser && !m.IsRead)
                .GroupBy(m => m.SenderUserName)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            ViewBag.CurrentUser = currentUser;
            ViewBag.UnreadCounts = unreadCounts;

            return View(users);
        }
        [HttpGet]
        public async Task<IActionResult> GetChatHistory(string withUser)
        {
            var currentUser = User.Identity?.Name;

            var messages = await _context.ChatMessages
                .Where(m => (m.SenderUserName == currentUser && m.ReceiverUserName == withUser)
                         || (m.SenderUserName == withUser && m.ReceiverUserName == currentUser))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderUserName == withUser && m.ReceiverUserName == currentUser && !m.IsRead)
                .ToListAsync();

            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
            }
            await _context.SaveChangesAsync();

            var messageViewModel = new List<ChatMessageViewModel>();

            var receiverUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == withUser);

            foreach (var msg in messages)
            {
                var sender = await _context.Users.FirstOrDefaultAsync(u => u.UserName == msg.SenderUserName);
                var role = sender?.Role;

                messageViewModel.Add(new ChatMessageViewModel
                {
                    SenderUserName = msg.SenderUserName,
                    SenderUserEmail = sender?.Email,
                    SenderUserId = sender?.Id,
                    SenderRole = role,
                    Message = msg.Message,
                    Timestamp = msg.Timestamp,
                    IsCurrentUser = msg.SenderUserName == currentUser,
                    ProfilePicturePath = sender?.ProfilePicturePath ?? "/images/anonymous.png",
                    IsOnline = receiverUser?.IsOnline ?? false,
                    LastSeen = receiverUser?.LastSeen
                });
            }

            ViewBag.CurrentUser = currentUser;
            return PartialView("_ChatHistory", messageViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(string withUser)
        {
            var currentUser = User.Identity?.Name;
            var messages = await _context.ChatMessages
                .Where(m => m.SenderUserName == withUser && m.ReceiverUserName == currentUser)
                .ToListAsync();
            return Json(new { success = true });
        }
    }
}