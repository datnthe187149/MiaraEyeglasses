namespace MiaraEyeglasses.Models;

// Model chứa thông tin lỗi hiển thị trên trang Error chung của hệ thống
public class ErrorViewModel
{
    // Mã định danh của request gặp lỗi (RequestId) để tra cứu nhật ký
    public string? RequestId { get; set; }

    // Cờ kiểm tra có hiển thị mã RequestId trên giao diện hay không
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
