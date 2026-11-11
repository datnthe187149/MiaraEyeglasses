using System.ComponentModel.DataAnnotations;

namespace MiaraEyeglasses.Models.ViewModels
{
    // Model cập nhật thông tin cá nhân của người dùng
    public class UpdateProfileViewModel
    {
        // Họ và tên người dùng (bắt buộc, tối đa 100 ký tự)
        [Required(ErrorMessage = "Vui lòng nhập Họ và tên")]
        [StringLength(100, ErrorMessage = "Họ tên không vượt quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        // Số điện thoại liên hệ (bắt buộc, đúng định dạng số điện thoại)
        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        // Giới tính (Nam, Nữ, Khác)
        public string? Gender { get; set; }

        // Ngày tháng năm sinh của người dùng
        public DateOnly? BirthDate { get; set; }
    }

    // Model đổi mật khẩu tài khoản
    public class ChangePasswordViewModel
    {
        // Mật khẩu hiện tại của tài khoản
        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        // Mật khẩu mới mong muốn (tối thiểu 6 ký tự)
        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        // Xác nhận mật khẩu mới (phải khớp với mật khẩu mới)
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // Model thêm hoặc chỉnh sửa sổ địa chỉ nhận hàng
    public class AddAddressViewModel
    {
        // Họ và tên người nhận hàng
        [Required(ErrorMessage = "Vui lòng nhập Họ tên người nhận")]
        [StringLength(100)]
        public string RecipientName { get; set; } = string.Empty;

        // Số điện thoại người nhận hàng
        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        // Tỉnh hoặc Thành phố trực thuộc
        [Required(ErrorMessage = "Vui lòng nhập Tỉnh/Thành phố")]
        public string Province { get; set; } = string.Empty;

        // Quận / Huyện / Phường / Xã
        [Required(ErrorMessage = "Vui lòng nhập Quận/Huyện/Phường/Xã")]
        public string DistrictWard { get; set; } = string.Empty;

        // Số nhà, tên đường hoặc địa chỉ chi tiết
        [Required(ErrorMessage = "Vui lòng nhập Địa chỉ chi tiết (số nhà, đường)")]
        public string StreetAddress { get; set; } = string.Empty;

        // Cờ đánh dấu đây có phải là địa chỉ mặc định hay không
        public bool IsDefault { get; set; }
    }

    // Model thêm hồ sơ thông số đo mắt / độ cận viễn loạn
    public class AddPrescriptionViewModel
    {
        // Tên đặt cho hồ sơ độ mắt (ví dụ: Độ mắt đi làm, Kính cận học tập)
        [Required(ErrorMessage = "Vui lòng đặt tên hồ sơ độ")]
        public string ProfileName { get; set; } = "Hồ sơ độ cận chính";

        // Mắt phải: Độ cầu (Spherical)
        public decimal? RightSph { get; set; }
        // Mắt phải: Độ trụ / loạn thị (Cylindrical)
        public decimal? RightCyl { get; set; }
        // Mắt phải: Trục loạn (Axis 0 - 180 độ)
        public short? RightAxis { get; set; }

        // Mắt trái: Độ cầu (Spherical)
        public decimal? LeftSph { get; set; }
        // Mắt trái: Độ trụ / loạn thị (Cylindrical)
        public decimal? LeftCyl { get; set; }
        // Mắt trái: Trục loạn (Axis 0 - 180 độ)
        public short? LeftAxis { get; set; }

        // Khoảng cách đồng tử (Pupillary Distance - PD tính bằng mm)
        public decimal? PD { get; set; }
        // Đường dẫn ảnh chụp đơn thuốc hoặc phiếu đo thị lực
        public string? PrescriptionImg { get; set; }
        // Cờ đánh dấu hồ sơ độ mắt mặc định
        public bool IsDefault { get; set; }
    }

    // Model gửi đánh giá và nhận xét sản phẩm từ khách hàng
    public class SubmitReviewViewModel
    {
        // ID sản phẩm được đánh giá
        public int ProductId { get; set; }

        // Số sao đánh giá (từ 1 đến 5 sao)
        [Range(1, 5, ErrorMessage = "Đánh giá từ 1 đến 5 sao")]
        public byte Rating { get; set; } = 5;

        // Nội dung nhận xét chi tiết của người dùng
        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        [StringLength(1000, ErrorMessage = "Nội dung không vượt quá 1000 ký tự")]
        public string Comment { get; set; } = string.Empty;
    }
}
