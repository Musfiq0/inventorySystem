using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    public class SiteContent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Value { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public string? UpdatedBy { get; set; }
    }
}