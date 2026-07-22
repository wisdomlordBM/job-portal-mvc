

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Jobportalwebsite.Models;
using Jobportalwebsite.Services;

namespace Jobportalwebsite.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Jobseekers> Jobseekers { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reply> Replies { get; set; } // Add Replies DbSet
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<JobSeekerAnswer> JobSeekerAnswers { get; set; }
        public DbSet<JobSkillTest> JobSkillTests { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Currency> Currencies { get; set; }

        // New Skill Assessment DbSets


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>()
                .HasIndex(country => country.IsoCode)
                .IsUnique();

            modelBuilder.Entity<Currency>()
                .HasIndex(currency => currency.Code)
                .IsUnique();

            modelBuilder.Entity<Country>()
                .HasOne(country => country.Currency)
                .WithMany()
                .HasForeignKey(country => country.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Company>()
                .HasOne(company => company.Country)
                .WithMany()
                .HasForeignKey(company => company.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(user => user.CountryReference)
                .WithMany()
                .HasForeignKey(user => user.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Job's Salary column
            modelBuilder.Entity<Job>()
                .Property(j => j.Salary)
                .HasColumnType("decimal(18,2)");

            // Configure the relationship between Comment and Reply
            modelBuilder.Entity<Reply>()
                .HasOne(r => r.Comment)
                .WithMany(c => c.Replies)
                .HasForeignKey(r => r.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobSeekerAnswer>()
                 .HasOne(j => j.Application)
                 .WithMany(a => a.JobSeekerAnswers)
                 .HasForeignKey(j => j.ApplicationId)
                 .OnDelete(DeleteBehavior.NoAction);  // Prevent cascading delete

            modelBuilder.Entity<JobSeekerAnswer>()
                .HasOne(j => j.JobSkillTest)
                .WithMany(q => q.JobSeekerAnswers)
                .HasForeignKey(j => j.JobSkillTestId)
                .OnDelete(DeleteBehavior.NoAction);  // Prevent cascading delete



            base.OnModelCreating(modelBuilder);
        }
    }
}





