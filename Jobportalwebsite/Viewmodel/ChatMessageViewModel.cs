namespace Jobportalwebsite.ViewModel
{
    public class ChatMessageViewModel
    {
        public string SenderUserName { get; set; }
        public string SenderUserId { get; set; }
       
        public string SenderRole { get; set; } // e.g., "Jobseeker" or "Company"
        public string SenderUserEmail { get; set; }
        public bool IsRead { get; set; } = false; // <-- NEW

        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsCurrentUser { get; set; }
        public string ProfilePicturePath { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }

    }
}
