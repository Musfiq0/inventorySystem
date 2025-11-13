using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InventoryId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [StringLength(100, ErrorMessage = "Item name cannot exceed 100 characters")]
        [Display(Name = "Item Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be zero or greater")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 0;

        /// <summary>
        /// Price per unit of this item
        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be zero or greater")]
        [Display(Name = "Price per Unit")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; } = 0;

        [StringLength(50, ErrorMessage = "Custom ID cannot exceed 50 characters")]
        [Display(Name = "Custom ID")]
        public string? CustomId { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [Display(Name = "Category")]
        public string? Category { get; set; }

        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        [Display(Name = "SKU")]
        public string? SKU { get; set; }

        [Required]
        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("InventoryId")]
        public virtual Inventory? Inventory { get; set; }

        [NotMapped]
        [Display(Name = "Total Value")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TotalValue => Price * Quantity;

        public static string GenerateCustomId(int itemCount)
        {
            return $"ITEM{(itemCount + 1):D3}";
        }

        [NotMapped]
        public bool IsLowStock => Quantity <= 5;

        [NotMapped]
        public bool IsOutOfStock => Quantity == 0;
    }
}