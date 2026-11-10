using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng Brands: Thương hiệu gọng kính (MIARA, PARZIN, KABAOLAI, ALFIO RALDO...)
    /// </summary>
    [Table("Brands")]
    public class Brand
    {
        [Key]
        public int BrandId { get; set; }

        [Required]
        [StringLength(100)]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

