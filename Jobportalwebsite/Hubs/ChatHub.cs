
using Microsoft.AspNetCore.SignalR;
using Jobportalwebsite.Data;
using Jobportalwebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace Jobportalwebsite.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string toUserName, string message)
        {
            var fromUserName = Context.User?.Identity?.Name;

            if (string.IsNullOrWhiteSpace(fromUserName) || string.IsNullOrWhiteSpace(toUserName) || string.IsNullOrWhiteSpace(message))
                return;

            var chatMessage = new ChatMessage
            {
                SenderUserName = fromUserName,
                ReceiverUserName = toUserName,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.Users(new[] { fromUserName, toUserName }).SendAsync(
                "ReceiveMessage",
                fromUserName,
                toUserName,
                message,
                chatMessage.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")
            );
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
                if (user != null)
                {
                    user.IsOnline = true;
                    user.LastSeen = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
                if (user != null)
                {
                    user.IsOnline = false;
                    user.LastSeen = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
