using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiaraEyeglasses.Models.Entities
{
    // Bảng Roles: Vai trò người dùng (1: Customer, 2: Staff, 3: Admin)
    [Table("Roles")]
    // Class định nghĩa: Role
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        // Thuộc tính: RoleId
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        // Thuộc tính: RoleName
        public string RoleName { get; set; } = string.Empty;

        // Navigation
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}

