using System;

namespace Jobportalwebsite.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderUserName { get; set; }
        public string ReceiverUserName { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false; // <-- NEW

    }
}

