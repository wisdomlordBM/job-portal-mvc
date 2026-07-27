using System;
using System.Collections.Generic;

namespace Jobportalwebsite.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<string> PicturePaths { get; set; } = new List<string>();
        public DateTime DateCreated { get; set; }
        public bool Deleted { get; set; }
        public string? CreatedBy { get; set; } 
        public int? CompanyId { get; set; }
        public virtual Company? Company { get; set; }
        public ICollection<Comment>? Comments { get; set; }
    }

    public class Comment
    {
        public int Id { get; set; }
        public string? CommentText { get; set; }
        public DateTime DateCommented { get; set; }
        public bool IsAnonymous { get; set; }
        public bool IsCompany { get; set; } // Distinguish entity type
        public string? UserId { get; set; } // Changed to string
        public int? CompanyId { get; set; } // For Company

        public string? UserRole { get; set; }
        public string? CommentedBy { get; set; }
        public string? ProfilePicturePath { get; set; } // Path to the profile picture or null for anonymous
        public int? BlogCompanyId { get; set; } // Company ID (Nullable)
        public string? EntityType { get; set; } // "Jobseeker" or "Company"
        public virtual User? Jobseeker { get; set; } // Assuming `User` represents a Jobseeker
        public virtual Company? Company { get; set; }
        public virtual ApplicationUser? User { get; set; } // Assuming ApplicationUser is your custom user model

        public int BlogId { get; set; }
        public Blog? Blog { get; set; }
        public ICollection<Reply>? Replies { get; set; } // List of replies
    }

    public class Reply
    {
        public int Id { get; set; }
        public string? ReplyText { get; set; }
        public DateTime DateReplied { get; set; }
        public bool IsAnonymous { get; set; }
        public bool IsCompany { get; set; } // Distinguish entity type

        public string? UserRole { get; set; }
        public string? RepliedBy { get; set; }
        public int? BlogCompanyId { get; set; } // Company ID (Nullable)
        public string? EntityType { get; set; } // "Jobseeker" or "Company"
        public virtual ApplicationUser? User { get; set; } // Assuming ApplicationUser is your custom user model

        public string? ProfilePicturePath { get; set; }
        public virtual User? Jobseeker { get; set; }
        public string? UserId { get; set; } // Changed to string

        public int? CompanyId { get; set; } // For Company
                                            // Assuming `User` represents a Jobseeker
        public virtual Company? Company { get; set; }
        public int CommentId { get; set; }
        public Comment? Comment { get; set; }
    }

 
}

