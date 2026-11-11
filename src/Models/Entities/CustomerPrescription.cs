using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng CustomerPrescriptions: Sổ lưu trữ hồ sơ độ cận cá nhân của khách hàng
    [Table("CustomerPrescriptions")]
    // Class định nghĩa: CustomerPrescription
    public class CustomerPrescription
    {
        [Key]
        // Thuộc tính: UserPrescriptionId
        public int UserPrescriptionId { get; set; }

        // Thuộc tính: UserId
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        // Thuộc tính: ProfileName
        public string ProfileName { get; set; } = "Độ cận chính của tôi";

        // Mắt phải (OD)
        [Column(TypeName = "decimal(4,2)")]
        // Thuộc tính: RightSph
        public decimal? RightSph { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        // Thuộc tính: RightCyl
        public decimal? RightCyl { get; set; }

        // Thuộc tính: RightAxis
        public short? RightAxis { get; set; }

        // Mắt trái (OS)
        [Column(TypeName = "decimal(4,2)")]
        // Thuộc tính: LeftSph
        public decimal? LeftSph { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        // Thuộc tính: LeftCyl
        public decimal? LeftCyl { get; set; }

        // Thuộc tính: LeftAxis
        public short? LeftAxis { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        // Thuộc tính: PD
        public decimal? PD { get; set; }

        [StringLength(300)]
        // Thuộc tính: PrescriptionImg
        public string? PrescriptionImg { get; set; }

        // Thuộc tính: IsDefault
        public bool IsDefault { get; set; } = false;

        // Thuộc tính: CreatedAt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

