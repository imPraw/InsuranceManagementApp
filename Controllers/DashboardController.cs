using InsuranceManagement.Web.Data;
using InsuranceManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.Roles = HttpContext.Session.GetString("Roles");
            ViewBag.UserId = userId;

            var model = GetDashboardStats(userId.Value, ViewBag.Roles ?? "");
            return View(model);
        }

        private DashboardViewModel GetDashboardStats(int userId, string roles)
        {
            var model = new DashboardViewModel();

            model.TotalPolicies = _context.InsurancePolicies.Count();

            // Use the new Status enum (not the old PolicyStatus string)
            model.ActivePolicies = _context.InsurancePolicies
                .Count(p => p.Status == PolicyStatus.Approved);

            model.PendingApplications = _context.InsurancePolicies
                .Count(p => p.Status == PolicyStatus.Pending);

            model.PendingClaims = _context.Claims
                .Count(c => c.Status == ClaimStatus.Submitted || c.Status == ClaimStatus.UnderReview);

            if (roles.Contains("Admin"))
            {
                model.TotalUsers = _context.AppUsers.Count();
                model.TotalRoles = _context.Roles.Count();
            }

            // Recent policies - for admin show all, for users show their own
            var query = _context.InsurancePolicies.Include(p => p.User).AsQueryable();
            if (!roles.Contains("Admin"))
                query = query.Where(p => p.UserId == userId);

            model.RecentPolicies = query
                .OrderByDescending(p => p.ApplicationDate)
                .Take(5)
                .ToList();

            return model;
        }
    }

    public class DashboardViewModel
    {
        public int TotalPolicies { get; set; }
        public int ActivePolicies { get; set; }
        public int PendingApplications { get; set; }
        public int PendingClaims { get; set; }
        public int TotalUsers { get; set; }
        public int TotalRoles { get; set; }
        public List<InsurancePolicy> RecentPolicies { get; set; } = new List<InsurancePolicy>();
    }
}
