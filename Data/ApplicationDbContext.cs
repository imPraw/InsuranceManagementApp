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
    }
}
