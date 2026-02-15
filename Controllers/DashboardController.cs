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

        // GET: /Dashboard/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var username = HttpContext.Session.GetString("Username");
            var roles = HttpContext.Session.GetString("Roles");

            ViewBag.Username = username;
            ViewBag.Roles = roles;
            ViewBag.UserId = userId;

            // Get dashboard statistics
            var model = GetDashboardStats(userId.Value, roles ?? "");

            return View(model);
        }

        private DashboardViewModel GetDashboardStats(int userId, string roles)
        {
            var model = new DashboardViewModel();

            // Get insurance policies count
            model.TotalPolicies = _context.InsurancePolicies.Count();

            // Get active policies
            model.ActivePolicies = _context.InsurancePolicies.Count(p => p.PolicyStatus == "Active");

            // Get pending claims
            model.PendingClaims = _context.InsurancePolicies.Count(p => p.PolicyStatus == "Pending");

            // Get total users (for admin)
            if (roles.Contains("Admin"))
            {
                model.TotalUsers = _context.AppUsers.Count();
                model.TotalRoles = _context.Roles.Count();
            }

            // Get recent policies
            model.RecentPolicies = _context.InsurancePolicies
                .Take(5)
                .ToList();

            return model;
        }
    }

    public class DashboardViewModel
    {
        public int TotalPolicies { get; set; }
        public int UserPoliciesCount { get; set; }
        public int ActivePolicies { get; set; }
        public int PendingClaims { get; set; }
        public int TotalUsers { get; set; }
        public int TotalRoles { get; set; }
        public List<InsurancePolicy> RecentPolicies { get; set; } = new List<InsurancePolicy>();
    }
}
