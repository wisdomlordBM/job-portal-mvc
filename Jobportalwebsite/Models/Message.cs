using System;
using System.ComponentModel.DataAnnotations;

namespace Jobportalwebsite.Models

{
    public class Message
    {
        public int MessageId { get; set; }

        [Required]
        public string SenderId { get; set; } // Maps to AspNetUsers Id

        [Required]
        public string ReceiverId { get; set; } // Maps to AspNetUsers Id

        [Required]
        public string MessageText { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
