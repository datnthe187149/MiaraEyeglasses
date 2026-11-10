using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng Users: Quản lý tài khoản khách hàng, nhân viên và quản trị viên
    /// </summary>
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Gender { get; set; } // 'Nữ', 'Nam', 'Khác'

        public DateOnly? BirthDate { get; set; }

        public int RoleId { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        [StringLength(64)]
        public string? ResetPasswordTokenHash { get; set; }

        public DateTime? ResetPasswordExpire { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

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

