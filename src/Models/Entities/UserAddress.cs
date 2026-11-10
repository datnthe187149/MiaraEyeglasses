using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng UserAddresses: Sổ địa chỉ giao hàng của khách hàng
    /// </summary>
    [Table("UserAddresses")]
    public class UserAddress
    {
        [Key]
        public int AddressId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Province { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string DistrictWard { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string StreetAddress { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

