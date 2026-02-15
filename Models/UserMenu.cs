using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsuranceManagement.Web.Models
{
    /// <summary>
    /// Junction table between User and Menu - determines which Menu is available to which users
    /// </summary>
    public class UserMenu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int MenuId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }

        [ForeignKey("MenuId")]
        public virtual Menu? Menu { get; set; }
    }
}
