using InsuranceManagement.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Web.Data
{
    // DbContext is the bridge between our C# code and the SQLite database.
    // It manages database connections and translates our C# operations into SQL.
    public class ApplicationDbContext : DbContext
    {
        // This constructor receives configuration from Program.cs
        // (like which database to use and where it's located).
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet represents the InsurancePolicies table in our database.
        // We use this to query, add, update, and delete records.
        // EF Core will create a table called "InsurancePolicies" based on this.
        public DbSet<InsurancePolicy> InsurancePolicies { get; set; }

        // Authentication/Authorization DbSets
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<UserMenu> UserMenus { get; set; }
    }
}