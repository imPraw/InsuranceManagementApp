using System;
using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Web.Models
{
    public class InsurancePolicy
    {
        public int Id { get; set; }

        [Required]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        public string PolicyHolderName { get; set; } = string.Empty;

        [Required]
        public string InsuranceType { get; set; } = string.Empty;

        public decimal CoverageAmount { get; set; }
        public decimal Premium { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required]
        public string PolicyStatus { get; set; } = string.Empty;
    }
}
