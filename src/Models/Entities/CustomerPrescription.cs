using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Bảng CustomerPrescriptions: Sổ lưu trữ hồ sơ độ cận cá nhân của khách hàng
    /// </summary>
    [Table("CustomerPrescriptions")]
    public class CustomerPrescription
    {
        [Key]
        public int UserPrescriptionId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string ProfileName { get; set; } = "Độ cận chính của tôi";

        // Mắt phải (OD)
        [Column(TypeName = "decimal(4,2)")]
        public decimal? RightSph { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? RightCyl { get; set; }

        public short? RightAxis { get; set; }

        // Mắt trái (OS)
        [Column(TypeName = "decimal(4,2)")]
        public decimal? LeftSph { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? LeftCyl { get; set; }

        public short? LeftAxis { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        public decimal? PD { get; set; }

        [StringLength(300)]
        public string? PrescriptionImg { get; set; }

        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

