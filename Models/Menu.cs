using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Web.Models
{
    /// <summary>
    /// Represents a menu item in the navigation
    /// </summary>
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? DisplayName { get; set; }

        [StringLength(255)]
        public string? Url { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        public int? ParentId { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Menu? Parent { get; set; }
        public virtual ICollection<Menu> SubMenus { get; set; } = new List<Menu>();
        public virtual ICollection<UserMenu> UserMenus { get; set; } = new List<UserMenu>();
    }
}
