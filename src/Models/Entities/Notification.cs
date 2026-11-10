using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng Notifications: Thông báo hệ thống cho Role (Staff/Admin) hoặc cho từng User cá nhân
    /// </summary>
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public int? TargetRoleId { get; set; }
        public int? TargetUserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Content { get; set; }

        [StringLength(300)]
        public string? LinkUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigations
        [ForeignKey("TargetRoleId")]
        public virtual Role? TargetRole { get; set; }

        [ForeignKey("TargetUserId")]
        public virtual User? TargetUser { get; set; }
    }
}

