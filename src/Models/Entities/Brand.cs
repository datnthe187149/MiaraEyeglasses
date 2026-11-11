using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng Brands: Thương hiệu gọng kính (MIARA, PARZIN, KABAOLAI, ALFIO RALDO...)
    [Table("Brands")]
    // Class định nghĩa: Brand
    public class Brand
    {
        [Key]
        // Thuộc tính: BrandId
        public int BrandId { get; set; }

        [Required]
        [StringLength(100)]
        // Thuộc tính: BrandName
        public string BrandName { get; set; } = string.Empty;

        [StringLength(100)]
        // Thuộc tính: Country
        public string? Country { get; set; }

        [StringLength(500)]
        // Thuộc tính: Description
        public string? Description { get; set; }

        // Thuộc tính: IsActive
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

