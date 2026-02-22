using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceManagement.Web.Models
{
    // Tracks the lifecycle of a claim submitted by a user
    public enum ClaimStatus
    {
        Submitted,    // User just filed it
        UnderReview,  // Admin is reviewing
        Approved,     // Admin approved - awaiting settlement
        Denied,       // Admin rejected the claim
        Settled       // Payment has been made
    }

    public class Claim
    {
        public int Id { get; set; }

        // Which policy this claim is filed against
        public int InsurancePolicyId { get; set; }

        // Who filed the claim
        public int UserId { get; set; }

        // Auto-generated unique claim reference number
        [Required]
        public string ClaimNumber { get; set; } = string.Empty;

        // User's description of what happened
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        // How much the user is claiming
        [Range(0.01, double.MaxValue, ErrorMessage = "Claim amount must be greater than 0")]
        public decimal ClaimAmount { get; set; }

        // When the incident/loss occurred
        [DataType(DataType.Date)]
        public DateTime IncidentDate { get; set; }

        // When the claim was submitted to the system
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        // Current status in the claims workflow
        public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;

        // --- Admin review fields ---
        public int? ReviewedByAdminId { get; set; }
        public DateTime? ReviewedAt { get; set; }

        [StringLength(1000)]
        public string? AdminRemarks { get; set; }

        // How much was actually paid out (may differ from claimed amount)
        public decimal? SettledAmount { get; set; }

        // Navigation properties
        [ForeignKey("InsurancePolicyId")]
        public virtual InsurancePolicy? InsurancePolicy { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }
    }
}
