using System;
using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Web.Models
{
    // This class represents a single insurance policy in our system.
    // Each property here will become a column in the database table.
    public class InsurancePolicy
    {
        // Primary key - uniquely identifies each policy in the database.
        // EF Core will auto-generate this value when we create new records.
        public int Id { get; set; }

        // [Required] means this field cannot be left empty.
        // This validation happens both client-side (browser) and server-side.
        [Required]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        public string PolicyHolderName { get; set; } = string.Empty;

        [Required]
        public string InsuranceType { get; set; } = string.Empty;

        // [Range] ensures the value is between 0 and the maximum possible decimal.
        // This prevents negative coverage amounts.
        [Range(0, double.MaxValue)]
        public decimal CoverageAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Premium { get; set; }

        // [DataType(DataType.Date)] tells the browser to show a date picker.
        // It also formats the date properly in views.
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public string PolicyStatus { get; set; } = string.Empty;
    }
}