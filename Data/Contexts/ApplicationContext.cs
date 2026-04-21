using Core.Concretes.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser, ApplicationUserRole, string>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Activity> Activities { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<Lead> Leads { get; set; }
        public virtual DbSet<Opportunity> Opportunities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Activity - Lead İlişkisi =====
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.RelatedLead)
                .WithMany(l => l.Activities)
                .HasForeignKey(a => a.RelatedLeadId)
                .OnDelete(DeleteBehavior.SetNull);

            // ===== Activity - Customer İlişkisi =====
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.RelatedCustomer)
                .WithMany()
                .HasForeignKey(a => a.RelatedCustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // ===== Activity - Opportunity İlişkisi =====
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.RelatedOpportunity)
                .WithMany()
                .HasForeignKey(a => a.RelatedOpportunityId)
                .OnDelete(DeleteBehavior.SetNull);

            // ===== Activity - ApplicationUser (AssignedUser) İlişkisi =====
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.AssignedUser)
                .WithMany()
                .HasForeignKey(a => a.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ===== Lead - ConvertedCustomer İlişkisi =====
            modelBuilder.Entity<Lead>()
                .HasOne(l => l.ConvertedCustomer)
                .WithMany()
                .HasForeignKey(l => l.ConvertedCustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // ===== Lead - AssignedUser İlişkisi =====
            modelBuilder.Entity<Lead>()
                .HasOne(l => l.AssignedUser)
                .WithMany()
                .HasForeignKey(l => l.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}