using InsuranceManagement.Web.Data;
using InsuranceManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Web.Controllers
{
    // This is a Web API controller - it returns JSON, not HTML views.
    // Route: /api/claims
    [ApiController]
    [Route("api/claims")]
    public class ClaimsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsApiController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper: Get logged-in user ID from session
        private int? GetSessionUserId()
            => _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");

        // Helper: Check if logged-in user is an Admin
        private bool IsAdmin()
        {
            var roles = _httpContextAccessor.HttpContext?.Session.GetString("Roles") ?? "";
            return roles.Contains("Admin");
        }

        // ─────────────────────────────────────────────
        // GET /api/claims
        // Returns all claims. Admins see all; users see only their own.
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status = null)
        {
            var userId = GetSessionUserId();
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in to view claims." });

            var query = _context.Claims
                .Include(c => c.InsurancePolicy)
                .Include(c => c.User)
                .AsQueryable();

            // Regular users only see their own claims
            if (!IsAdmin())
                query = query.Where(c => c.UserId == userId);

            // Optional filter by status (e.g. ?status=Submitted)
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ClaimStatus>(status, true, out var parsedStatus))
                query = query.Where(c => c.Status == parsedStatus);

            var claims = await query
                .OrderByDescending(c => c.SubmittedAt)
                .Select(c => new
                {
                    c.Id,
                    c.ClaimNumber,
                    c.Description,
                    c.ClaimAmount,
                    c.IncidentDate,
                    c.SubmittedAt,
                    Status = c.Status.ToString(),
                    c.AdminRemarks,
                    c.SettledAmount,
                    PolicyNumber = c.InsurancePolicy!.PolicyNumber,
                    PolicyType = c.InsurancePolicy.InsuranceType,
                    UserName = c.User!.Username,
                    UserFullName = c.User.FullName
                })
                .ToListAsync();

            return Ok(claims);
        }

        // ─────────────────────────────────────────────
        // GET /api/claims/{id}
        // Returns one specific claim by ID
        // ─────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetSessionUserId();
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            var claim = await _context.Claims
                .Include(c => c.InsurancePolicy)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
                return NotFound(new { message = $"Claim with ID {id} not found." });

            // Users can only view their own claims
            if (!IsAdmin() && claim.UserId != userId)
                return StatusCode(403, new { message = "You don't have permission to view this claim." });

            return Ok(new
            {
                claim.Id,
                claim.ClaimNumber,
                claim.Description,
                claim.ClaimAmount,
                claim.IncidentDate,
                claim.SubmittedAt,
                Status = claim.Status.ToString(),
                claim.AdminRemarks,
                claim.SettledAmount,
                claim.ReviewedAt,
                claim.InsurancePolicyId,
                PolicyNumber = claim.InsurancePolicy?.PolicyNumber,
                PolicyType = claim.InsurancePolicy?.InsuranceType,
                UserName = claim.User?.Username
            });
        }

        // ─────────────────────────────────────────────
        // POST /api/claims
        // Submit a new claim against an approved policy
        // Body: { "insurancePolicyId": 1, "description": "...", "claimAmount": 5000, "incidentDate": "2026-01-15" }
        // ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClaimRequest request)
        {
            var userId = GetSessionUserId();
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in to file a claim." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify the policy exists and belongs to this user
            var policy = await _context.InsurancePolicies
                .FirstOrDefaultAsync(p => p.Id == request.InsurancePolicyId && p.UserId == userId);

            if (policy == null)
                return BadRequest(new { message = "Policy not found or does not belong to your account." });

            // Only approved policies can have claims filed against them
            if (policy.Status != PolicyStatus.Approved)
                return BadRequest(new { message = $"Claims can only be filed on Approved policies. This policy is currently '{policy.Status}'." });

            var claim = new Claim
            {
                InsurancePolicyId = request.InsurancePolicyId,
                UserId = userId.Value,
                ClaimNumber = $"CLM-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                Description = request.Description,
                ClaimAmount = request.ClaimAmount,
                IncidentDate = request.IncidentDate,
                SubmittedAt = DateTime.Now,
                Status = ClaimStatus.Submitted
            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = claim.Id }, new
            {
                claim.Id,
                claim.ClaimNumber,
                message = "Claim submitted successfully. An admin will review it shortly."
            });
        }

        // ─────────────────────────────────────────────
        // PUT /api/claims/{id}
        // Update a claim (only allowed while still in Submitted status)
        // Body: { "description": "...", "claimAmount": 6000, "incidentDate": "2026-01-15" }
        // ─────────────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClaimRequest request)
        {
            var userId = GetSessionUserId();
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
                return NotFound(new { message = $"Claim with ID {id} not found." });

            // Only the claim owner can edit it
            if (claim.UserId != userId && !IsAdmin())
                return StatusCode(403, new { message = "You don't have permission to edit this claim." });

            // Can only edit claims that haven't been reviewed yet
            if (claim.Status != ClaimStatus.Submitted)
                return BadRequest(new { message = $"Only 'Submitted' claims can be edited. This claim is '{claim.Status}'." });

            claim.Description = request.Description;
            claim.ClaimAmount = request.ClaimAmount;
            claim.IncidentDate = request.IncidentDate;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Claim updated successfully.", claim.Id, claim.ClaimNumber });
        }

        // ─────────────────────────────────────────────
        // DELETE /api/claims/{id}
        // Withdraw/delete a claim (user can only withdraw their own Submitted claims)
        // ─────────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetSessionUserId();
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
                return NotFound(new { message = $"Claim with ID {id} not found." });

            // Admins can delete any claim; users can only delete their own
            if (!IsAdmin() && claim.UserId != userId)
                return StatusCode(403, new { message = "You don't have permission to delete this claim." });

            // Regular users can only withdraw claims they haven't submitted for review yet
            if (!IsAdmin() && claim.Status != ClaimStatus.Submitted)
                return BadRequest(new { message = "You can only withdraw claims that are still in 'Submitted' status." });

            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Claim {claim.ClaimNumber} has been withdrawn/deleted." });
        }

        // ─────────────────────────────────────────────
        // POST /api/claims/{id}/review
        // Admin only: Approve or Deny a claim
        // Body: { "approved": true, "remarks": "...", "settledAmount": 4500.00 }
        // ─────────────────────────────────────────────
        [HttpPost("{id}/review")]
        public async Task<IActionResult> Review(int id, [FromBody] ReviewClaimRequest request)
        {
            if (!IsAdmin())
                return StatusCode(403, new { message = "Only admins can review claims." });

            var userId = GetSessionUserId();
            var claim = await _context.Claims.FindAsync(id);

            if (claim == null)
                return NotFound(new { message = $"Claim with ID {id} not found." });

            if (claim.Status == ClaimStatus.Settled || claim.Status == ClaimStatus.Denied)
                return BadRequest(new { message = $"This claim has already been finalized with status '{claim.Status}'." });

            claim.ReviewedByAdminId = userId;
            claim.ReviewedAt = DateTime.Now;
            claim.AdminRemarks = request.Remarks;

            if (request.Approved)
            {
                // If a settled amount is provided, mark as Settled; otherwise just Approved
                claim.Status = request.SettledAmount.HasValue ? ClaimStatus.Settled : ClaimStatus.Approved;
                claim.SettledAmount = request.SettledAmount;
            }
            else
            {
                claim.Status = ClaimStatus.Denied;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Claim {claim.ClaimNumber} has been {claim.Status}.",
                claim.Id,
                claim.ClaimNumber,
                Status = claim.Status.ToString(),
                claim.SettledAmount
            });
        }
    }

    // ─────────────────────────────────────────────
    // Request DTOs (Data Transfer Objects)
    // These define the shape of the JSON body for each endpoint
    // ─────────────────────────────────────────────

    public class CreateClaimRequest
    {
        [Required]
        public int InsurancePolicyId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Claim amount must be greater than 0.")]
        public decimal ClaimAmount { get; set; }

        [Required]
        public DateTime IncidentDate { get; set; }
    }

    public class UpdateClaimRequest
    {
        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal ClaimAmount { get; set; }

        [Required]
        public DateTime IncidentDate { get; set; }
    }

    public class ReviewClaimRequest
    {
        [Required]
        public bool Approved { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        // Optional: if provided and Approved=true, claim goes straight to Settled
        [Range(0.01, double.MaxValue)]
        public decimal? SettledAmount { get; set; }
    }
}
