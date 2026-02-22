using InsuranceManagement.Web.Data;
using InsuranceManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Web.Controllers
{
    public class InsuranceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InsuranceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper: get logged-in user's ID from session. Returns null if not logged in.
        private int? GetSessionUserId() => HttpContext.Session.GetInt32("UserId");

        // Helper: check if the logged-in user is an Admin
        private bool IsAdmin()
        {
            var roles = HttpContext.Session.GetString("Roles") ?? "";
            return roles.Contains("Admin");
        }

        // ─────────────────────────────────────────────
        // USER SECTION: Apply for a policy, view own policies
        // ─────────────────────────────────────────────

        // GET: /Insurance
        // Regular users see their own policies. Admins see all policies.
        public async Task<IActionResult> Index()
        {
            var userId = GetSessionUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            IQueryable<InsurancePolicy> query = _context.InsurancePolicies
                .Include(p => p.User);

            // Admins see everything; regular users only see their own
            if (!IsAdmin())
                query = query.Where(p => p.UserId == userId);

            var policies = await query
                .OrderByDescending(p => p.ApplicationDate)
                .ToListAsync();

            ViewBag.IsAdmin = IsAdmin();
            return View(policies);
        }

        // GET: /Insurance/Apply
        // Shows the policy application form for regular users
        public IActionResult Apply()
        {
            if (GetSessionUserId() == null) return RedirectToAction("Login", "Account");
            return View();
        }

        // POST: /Insurance/Apply
        // Handles submission of a new policy application
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(InsurancePolicy policy)
        {
            var userId = GetSessionUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            // Remove validation for fields we'll set programmatically
            ModelState.Remove("PolicyNumber");
            ModelState.Remove("UserId");
            ModelState.Remove("Status");

            if (!ModelState.IsValid) return View(policy);

            // Set fields the user shouldn't control
            policy.UserId = userId.Value;
            policy.ApplicationDate = DateTime.Now;
            policy.Status = PolicyStatus.Pending;

            // Auto-generate a unique policy number: POL-YYYYMMDD-XXXXXX
            policy.PolicyNumber = $"POL-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            _context.InsurancePolicies.Add(policy);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Application submitted! Your reference number is {policy.PolicyNumber}. An admin will review your application shortly.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Insurance/Details/5
        // View a single policy and all its claims
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetSessionUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var policy = await _context.InsurancePolicies
                .Include(p => p.User)
                .Include(p => p.Claims)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (policy == null) return NotFound();

            // Users can only view their own policies (admins can view all)
            if (!IsAdmin() && policy.UserId != userId)
                return Forbid();

            ViewBag.IsAdmin = IsAdmin();
            return View(policy);
        }

        // ─────────────────────────────────────────────
        // ADMIN SECTION: Review and approve/deny policies
        // ─────────────────────────────────────────────

        // GET: /Insurance/AdminDashboard
        // Admin-only page to review all policy applications
        public async Task<IActionResult> AdminDashboard(string filter = "Pending")
        {
            if (!IsAdmin()) return Forbid();

            // Build query filtered by status
            var query = _context.InsurancePolicies
                .Include(p => p.User)
                .AsQueryable();

            if (Enum.TryParse<PolicyStatus>(filter, out var statusFilter))
                query = query.Where(p => p.Status == statusFilter);

            // Count badges for the tab headers
            ViewBag.Filter = filter;
            ViewBag.PendingCount = await _context.InsurancePolicies.CountAsync(p => p.Status == PolicyStatus.Pending);
            ViewBag.ApprovedCount = await _context.InsurancePolicies.CountAsync(p => p.Status == PolicyStatus.Approved);
            ViewBag.DeniedCount = await _context.InsurancePolicies.CountAsync(p => p.Status == PolicyStatus.Denied);

            return View(await query.OrderByDescending(p => p.ApplicationDate).ToListAsync());
        }

        // POST: /Insurance/ReviewPolicy
        // Admin approves or denies a policy application
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewPolicy(int id, string decision, string? remarks)
        {
            if (!IsAdmin()) return Forbid();

            var policy = await _context.InsurancePolicies.FindAsync(id);
            if (policy == null) return NotFound();

            var adminId = GetSessionUserId()!.Value;

            // Update the policy with the admin's decision
            policy.Status = decision == "Approve" ? PolicyStatus.Approved : PolicyStatus.Denied;
            policy.ReviewedByAdminId = adminId;
            policy.ReviewedAt = DateTime.Now;
            policy.AdminRemarks = remarks;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Policy {policy.PolicyNumber} has been {policy.Status}.";
            return RedirectToAction(nameof(AdminDashboard));
        }

        // ─────────────────────────────────────────────
        // CLAIM FILING (MVC - from the UI)
        // ─────────────────────────────────────────────

        // GET: /Insurance/FileClaim/5 (policyId)
        // Shows form to file a claim against an approved policy
        public async Task<IActionResult> FileClaim(int id)
        {
            var userId = GetSessionUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var policy = await _context.InsurancePolicies.FindAsync(id);
            if (policy == null) return NotFound();

            // Can only file claims on your own approved policies
            if (policy.UserId != userId)
                return Forbid();

            if (policy.Status != PolicyStatus.Approved)
            {
                TempData["Error"] = "You can only file claims on approved policies.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.Policy = policy;
            return View(new Claim { InsurancePolicyId = id });
        }

        // POST: /Insurance/FileClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FileClaim(Claim claim)
        {
            var userId = GetSessionUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            ModelState.Remove("ClaimNumber");
            ModelState.Remove("UserId");
            ModelState.Remove("Status");

            var policy = await _context.InsurancePolicies.FindAsync(claim.InsurancePolicyId);
            if (policy == null || policy.UserId != userId || policy.Status != PolicyStatus.Approved)
            {
                TempData["Error"] = "Invalid policy for claim submission.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Policy = policy;
                return View(claim);
            }

            claim.UserId = userId.Value;
            claim.SubmittedAt = DateTime.Now;
            claim.Status = ClaimStatus.Submitted;
            claim.ClaimNumber = $"CLM-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Claim {claim.ClaimNumber} submitted successfully!";
            return RedirectToAction(nameof(Details), new { id = claim.InsurancePolicyId });
        }

        // GET: /Insurance/MyClaims
        // User views all their claims across all policies
        public async Task<IActionResult> MyClaims()
        {
            var userId = GetSessionUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var claims = await _context.Claims
                .Include(c => c.InsurancePolicy)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View(claims);
        }

        // GET: /Insurance/AllClaims (admin only)
        public async Task<IActionResult> AllClaims(string filter = "")
        {
            if (!IsAdmin()) return Forbid();

            var query = _context.Claims
                .Include(c => c.InsurancePolicy)
                .Include(c => c.User)
                .AsQueryable();

            if (Enum.TryParse<ClaimStatus>(filter, out var statusFilter))
                query = query.Where(c => c.Status == statusFilter);

            ViewBag.Filter = filter;
            return View(await query.OrderByDescending(c => c.SubmittedAt).ToListAsync());
        }

        // POST: /Insurance/ReviewClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewClaim(int id, string decision, string? remarks, decimal? settledAmount)
        {
            if (!IsAdmin()) return Forbid();

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.ReviewedByAdminId = GetSessionUserId()!.Value;
            claim.ReviewedAt = DateTime.Now;
            claim.AdminRemarks = remarks;

            if (decision == "Approve")
            {
                claim.Status = settledAmount.HasValue ? ClaimStatus.Settled : ClaimStatus.Approved;
                claim.SettledAmount = settledAmount;
            }
            else
            {
                claim.Status = ClaimStatus.Denied;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Claim {claim.ClaimNumber} has been {claim.Status}.";
            return RedirectToAction(nameof(AllClaims));
        }

        // ─────────────────────────────────────────────
        // LEGACY CRUD (kept for backwards compatibility)
        // ─────────────────────────────────────────────

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return Forbid();
            var policy = await _context.InsurancePolicies.FindAsync(id);
            if (policy == null) return NotFound();
            return View(policy);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(InsurancePolicy policy)
        {
            if (!IsAdmin()) return Forbid();
            if (!ModelState.IsValid) return View(policy);
            _context.Update(policy);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return Forbid();
            var policy = await _context.InsurancePolicies.FindAsync(id);
            if (policy == null) return NotFound();
            return View(policy);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return Forbid();
            var policy = await _context.InsurancePolicies.FindAsync(id);
            if (policy != null)
            {
                _context.InsurancePolicies.Remove(policy);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
