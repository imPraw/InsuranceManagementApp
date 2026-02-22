using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceManagement.Web.Models
{
    // Enum to track where a policy is in the approval workflow.
    public enum PolicyStatus
    {
        Pending,   // Just applied, waiting for admin review
        Approved,  // Admin approved - user can now file claims
        Denied,    // Admin rejected the application
        Cancelled  // Cancelled
    }

    public class InsurancePolicy
    {
        public int Id { get; set; }

        // Links this policy to the user who applied for it
        public int UserId { get; set; }

        [Required]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        public string PolicyHolderName { get; set; } = string.Empty;

        [Required]
        public string InsuranceType { get; set; } = string.Empty;

        // What the user is applying for - describes their situation
        [Required]
        [StringLength(1000)]
        public string ApplicationDescription { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal CoverageAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Premium { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // When the user submitted the application
        public DateTime ApplicationDate { get; set; } = DateTime.Now;

        // Current status in the workflow (Pending -> Approved or Denied)
        public PolicyStatus Status { get; set; } = PolicyStatus.Pending;

        // Which admin reviewed this policy
        public int? ReviewedByAdminId { get; set; }

        // When the admin made their decision
        public DateTime? ReviewedAt { get; set; }

        // Admin's notes/reason for approval or denial
        [StringLength(1000)]
        public string? AdminRemarks { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }

        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
