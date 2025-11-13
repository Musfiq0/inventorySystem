using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Inventory name is required")]
        [StringLength(100, ErrorMessage = "Inventory name cannot exceed 100 characters")]
        [Display(Name = "Inventory Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Item> Items { get; set; } = new List<Item>();

        [NotMapped]
        [Display(Name = "Total Items")]
        public int TotalItems => Items?.Count ?? 0;

        [NotMapped]
        [Display(Name = "Total Value")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TotalValue => Items?.Sum(i => i.Price * i.Quantity) ?? 0;

        [NotMapped]
        [Display(Name = "Total Quantity")]
        public int TotalQuantity => Items?.Sum(i => i.Quantity) ?? 0;
    }
}