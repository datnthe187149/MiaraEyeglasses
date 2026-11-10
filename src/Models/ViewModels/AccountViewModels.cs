using System.ComponentModel.DataAnnotations;

namespace MiaraEyeglasses.Models.ViewModels
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Họ và tên")]
        [StringLength(100, ErrorMessage = "Họ tên không vượt quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        public string? Gender { get; set; }
        public DateOnly? BirthDate { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class AddAddressViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Họ tên người nhận")]
        [StringLength(100)]
        public string RecipientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Tỉnh/Thành phố")]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Quận/Huyện/Phường/Xã")]
        public string DistrictWard { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Địa chỉ chi tiết (số nhà, đường)")]
        public string StreetAddress { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
    }

    public class AddPrescriptionViewModel
    {
        [Required(ErrorMessage = "Vui lòng đặt tên hồ sơ độ")]
        public string ProfileName { get; set; } = "Hồ sơ độ cận chính";

        public decimal? RightSph { get; set; }
        public decimal? RightCyl { get; set; }
        public short? RightAxis { get; set; }

        public decimal? LeftSph { get; set; }
        public decimal? LeftCyl { get; set; }
        public short? LeftAxis { get; set; }

        public decimal? PD { get; set; }
        public string? PrescriptionImg { get; set; }
        public bool IsDefault { get; set; }
    }

    public class SubmitReviewViewModel
    {
        public int ProductId { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá từ 1 đến 5 sao")]
        public byte Rating { get; set; } = 5;

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        [StringLength(1000, ErrorMessage = "Nội dung không vượt quá 1000 ký tự")]
        public string Comment { get; set; } = string.Empty;
    }
}

