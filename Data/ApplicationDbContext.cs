using InsuranceManagement.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<InsurancePolicy> InsurancePolicies { get; set; }
        public DbSet<Claim> Claims { get; set; }  // NEW: Claims table

        // Auth/Authorization DbSets
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<UserMenu> UserMenus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Store enum as string so the database is human-readable
            modelBuilder.Entity<InsurancePolicy>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Claim>()
                .Property(c => c.Status)
                .HasConversion<string>();

            // A policy belongs to one user; a user can have many policies
            modelBuilder.Entity<InsurancePolicy>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // A claim belongs to one policy; a policy can have many claims
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.InsurancePolicy)
                .WithMany(p => p.Claims)
                .HasForeignKey(c => c.InsurancePolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            // A claim also has a user (the claimant) - use NoAction to avoid
            // multiple cascade paths which SQLite doesn't handle well
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
