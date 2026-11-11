using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng Notifications: Thông báo hệ thống cho Role (Staff/Admin) hoặc cho từng User cá nhân
    [Table("Notifications")]
    // Class định nghĩa: Notification
    public class Notification
    {
        [Key]
        // Thuộc tính: NotificationId
        public int NotificationId { get; set; }

        // Thuộc tính: TargetRoleId
        public int? TargetRoleId { get; set; }
        // Thuộc tính: TargetUserId
        public int? TargetUserId { get; set; }

        [Required]
        [StringLength(200)]
        // Thuộc tính: Title
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        // Thuộc tính: Content
        public string? Content { get; set; }

        [StringLength(300)]
        // Thuộc tính: LinkUrl
        public string? LinkUrl { get; set; }

        // Thuộc tính: IsRead
        public bool IsRead { get; set; } = false;

        // Thuộc tính: CreatedAt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigations
        [ForeignKey("TargetRoleId")]
        public virtual Role? TargetRole { get; set; }

        [ForeignKey("TargetUserId")]
        public virtual User? TargetUser { get; set; }
    }
}

