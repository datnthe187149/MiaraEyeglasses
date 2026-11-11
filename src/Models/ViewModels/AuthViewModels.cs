using System.ComponentModel.DataAnnotations;

namespace MiaraEyeglasses.Models.ViewModels
{
    // Model đăng nhập hệ thống cho người dùng và quản trị viên
    public class LoginViewModel
    {
        // Địa chỉ email tài khoản
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        // Mật khẩu đăng nhập
        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Ghi nhớ trạng thái đăng nhập trên trình duyệt
        public bool RememberMe { get; set; }

        // Đường dẫn URL muốn quay lại sau khi đăng nhập thành công
        public string? ReturnUrl { get; set; }
    }

    // Model đăng ký tài khoản khách hàng mới
    public class RegisterViewModel
    {
        // Họ và tên đầy đủ của người đăng ký
        [Required(ErrorMessage = "Vui lòng nhập Họ và tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        // Địa chỉ email dùng để kích hoạt và nhận thông báo đơn hàng
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        // Số điện thoại liên lạc
        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        // Mật khẩu tài khoản (tối thiểu 6 ký tự)
        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Xác nhận lại mật khẩu vừa nhập
        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Giới tính khách hàng
        public string? Gender { get; set; }

        // Ngày sinh
        public DateOnly? BirthDate { get; set; }
    }

    // Model yêu cầu gửi email lấy lại mật khẩu đã quên
    public class ForgotPasswordViewModel
    {
        // Địa chỉ email đã đăng ký cần nhận link đặt lại mật khẩu
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;
    }

    // Model đặt lại mật khẩu mới thông qua Session (chuẩn CatBaBooking)
    public class ResetPasswordViewModel
    {
        // Email của tài khoản đang cần đặt lại mật khẩu (lưu trong Session)
        public string? Email { get; set; }

        // Mật khẩu mới cần thiết lập
        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        // Xác nhận mật khẩu mới
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
