using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng UserAddresses: Sổ địa chỉ giao hàng của khách hàng
    [Table("UserAddresses")]
    // Class định nghĩa: UserAddress
    public class UserAddress
    {
        [Key]
        // Thuộc tính: AddressId
        public int AddressId { get; set; }

        // Thuộc tính: UserId
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        // Thuộc tính: RecipientName
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        // Thuộc tính: Phone
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        // Thuộc tính: Province
        public string Province { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        // Thuộc tính: DistrictWard
        public string DistrictWard { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        // Thuộc tính: StreetAddress
        public string StreetAddress { get; set; } = string.Empty;

        // Thuộc tính: IsDefault
        public bool IsDefault { get; set; } = false;

        // Thuộc tính: CreatedAt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

