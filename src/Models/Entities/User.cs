using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng Users: Quản lý tài khoản khách hàng, nhân viên và quản trị viên
    [Table("Users")]
    // Class định nghĩa: User
    public class User
    {
        [Key]
        // Thuộc tính: UserId
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        // Thuộc tính: FullName
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [EmailAddress]
        // Thuộc tính: Email
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Phone]
        // Thuộc tính: Phone
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        // Thuộc tính: PasswordHash
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(10)]
        // Thuộc tính: Gender
        public string? Gender { get; set; } // 'Nữ', 'Nam', 'Khác'

        // Thuộc tính: BirthDate
        public DateOnly? BirthDate { get; set; }

        // Thuộc tính: RoleId
        public int RoleId { get; set; } = 1;

        // Thuộc tính: IsActive
        public bool IsActive { get; set; } = true;

        [StringLength(64)]
        // Thuộc tính: ResetPasswordTokenHash
        public string? ResetPasswordTokenHash { get; set; }

        // Thuộc tính: ResetPasswordExpire
        public DateTime? ResetPasswordExpire { get; set; }

        // Thuộc tính: CreatedAt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Thuộc tính: UpdatedAt
        public DateTime? UpdatedAt { get; set; }

        // Navigations
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        public virtual ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
        public virtual ICollection<CustomerPrescription> Prescriptions { get; set; } = new List<CustomerPrescription>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}

