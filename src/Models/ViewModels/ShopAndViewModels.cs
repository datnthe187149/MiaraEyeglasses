using System.ComponentModel.DataAnnotations;
using MiaraEyeglasses.Models.Entities;

namespace MiaraEyeglasses.Models.ViewModels
{
    // Model chứa thông số độ cận, viễn, loạn thị khi khách đặt tròng kính
    public class PrescriptionInputModel
    {
        // Mắt phải: Độ cầu (từ -15.00 đến +10.00 diop)
        [Range(-15.00, 10.00, ErrorMessage = "Độ cầu mắt phải từ -15.00 đến +10.00")]
        public decimal? RightSph { get; set; }

        // Mắt phải: Độ loạn thị (từ -6.00 đến +6.00 diop)
        [Range(-6.00, 6.00, ErrorMessage = "Độ trụ mắt phải từ -6.00 đến +6.00")]
        public decimal? RightCyl { get; set; }

        // Mắt phải: Trục loạn thị (từ 0 đến 180 độ)
        [Range(0, 180, ErrorMessage = "Trục mắt phải từ 0 đến 180")]
        public short? RightAxis { get; set; }

        // Mắt trái: Độ cầu (từ -15.00 đến +10.00 diop)
        [Range(-15.00, 10.00, ErrorMessage = "Độ cầu mắt trái từ -15.00 đến +10.00")]
        public decimal? LeftSph { get; set; }

        // Mắt trái: Độ loạn thị (từ -6.00 đến +6.00 diop)
        [Range(-6.00, 6.00, ErrorMessage = "Độ trụ mắt trái từ -6.00 đến +6.00")]
        public decimal? LeftCyl { get; set; }

        // Mắt trái: Trục loạn thị (từ 0 đến 180 độ)
        [Range(0, 180, ErrorMessage = "Trục mắt trái từ 0 đến 180")]
        public short? LeftAxis { get; set; }

        // Khoảng cách đồng tử PD (tính bằng mm, thông thường từ 50 đến 78mm)
        [Range(50.0, 78.0, ErrorMessage = "Khoảng cách đồng tử PD từ 50 đến 78mm")]
        public decimal? PD { get; set; }

        // Đường dẫn ảnh chụp đơn thuốc hoặc phiếu đo thị lực
        public string? PrescriptionImg { get; set; }

        // Ghi chú thêm cho kỹ thuật viên mài lắp tròng kính
        public string? Note { get; set; }
    }

    // Model dữ liệu khi người dùng bấm Thêm vào giỏ hoặc Mua ngay
    public class AddToCartRequest
    {
        // ID của biến thể gọng kính (màu sắc, kích cỡ)
        [Required]
        public int VariantId { get; set; }

        // ID tùy chọn loại tròng kính (1 = Gọng không độ / Mẫu)
        public int LensOptionId { get; set; } = 1;

        // Số lượng kính muốn mua
        [Range(1, 20, ErrorMessage = "Số lượng phải từ 1 đến 20")]
        public int Quantity { get; set; } = 1;

        // Thông số độ khúc xạ đi kèm nếu có đặt cắt tròng có độ
        public PrescriptionInputModel? Prescription { get; set; }

        // Cờ đánh dấu nếu chọn Mua ngay để chuyển hướng thẳng sang trang Thanh toán
        public bool BuyNow { get; set; } = false;

        // Mã giảm giá áp dụng sớm nếu có
        public string? CouponCode { get; set; }
    }

    // Model chi tiết của từng món hàng trong giỏ hàng
    public class CartItemViewModel
    {
        // Mã định danh bản ghi CartItem trong cơ sở dữ liệu
        public int CartItemId { get; set; }
        // Mã biến thể kính
        public int VariantId { get; set; }
        // Mã sản phẩm gốc
        public int ProductId { get; set; }
        // Tên mẫu kính
        public string ProductName { get; set; } = string.Empty;
        // Mã SKU biến thể
        public string Sku { get; set; } = string.Empty;
        // Tên màu sắc gọng kính
        public string ColorName { get; set; } = string.Empty;
        // Kích thước gọng kính (S, M, L)
        public string SizeLabel { get; set; } = string.Empty;
        // Đường dẫn ảnh đại diện
        public string? ImageUrl { get; set; }
        // Giá bán của gọng kính
        public decimal FramePrice { get; set; }
        // ID tùy chọn tròng kính
        public int LensOptionId { get; set; }
        // Tên loại tròng kính đã chọn
        public string LensName { get; set; } = string.Empty;
        // Giá bán của tròng kính
        public decimal LensPrice { get; set; }
        // Số lượng đặt mua
        public int Quantity { get; set; }
        // Trạng thái được tích chọn để thanh toán
        public bool IsSelected { get; set; }
        // Tổng tiền của dòng sản phẩm: (Giá gọng + Giá tròng) * Số lượng
        public decimal LineTotal => (FramePrice + LensPrice) * Quantity;
        // Thông số độ khúc xạ đi kèm
        public PrescriptionInputModel? Prescription { get; set; }
    }

    // Model tổng hợp toàn bộ giỏ hàng và tính toán chi phí
    public class CartViewModel
    {
        // Danh sách các món hàng trong giỏ
        public List<CartItemViewModel> Items { get; set; } = new();
        // Tạm tính tiền hàng: Tổng của các sản phẩm được tích chọn
        public decimal Subtotal => Items.Where(i => i.IsSelected).Sum(i => i.LineTotal);
        // Số tiền được giảm từ mã khuyến mãi
        public decimal DiscountAmount { get; set; } = 0;
        // Phí vận chuyển giao hàng
        public decimal ShippingFee { get; set; } = 0;
        // Tổng tiền thanh toán cuối cùng: Tạm tính - Giảm giá + Phí ship
        public decimal TotalAmount => Subtotal - DiscountAmount + ShippingFee;
        // Mã giảm giá đang được áp dụng
        public string? AppliedCouponCode { get; set; }
    }

    // Model trang nhập thông tin nhận hàng và thanh toán (Checkout)
    public class CheckoutViewModel
    {
        // Họ và tên người nhận hàng
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        public string RecipientName { get; set; } = string.Empty;

        // Số điện thoại liên hệ nhận hàng
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        // Địa chỉ nhận hàng chi tiết
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        public string ShippingAddress { get; set; } = string.Empty;

        // Ghi chú của khách hàng khi giao kính
        public string? OrderNote { get; set; }

        // Mã giảm giá áp dụng
        public string? CouponCode { get; set; }

        // Phương thức thanh toán: Mặc định tự động chuyển sang VNPAY Sandbox
        public string PaymentMethod { get; set; } = "VNPAY";

        // Dữ liệu giỏ hàng để hiển thị tóm tắt chi phí
        public CartViewModel Cart { get; set; } = new();
    }

    // Model bộ lọc tìm kiếm sản phẩm cho trang danh mục và cửa hàng
    public class ProductFilterViewModel
    {
        // Từ khóa tìm kiếm theo tên hoặc mã sản phẩm
        public string? Search { get; set; }
        // Tiêu chí sắp xếp giá cả, mới nhất
        public string? Sort { get; set; }
        // Lọc theo mã danh mục
        public int? CategoryId { get; set; }
        // Lọc theo thương hiệu
        public int? BrandId { get; set; }
        // Lọc theo giới tính phù hợp
        public string? Gender { get; set; }
        // Lọc theo dáng mắt kính
        public string? Shape { get; set; }
        // Lọc theo chất liệu gọng kính
        public string? Material { get; set; }
        // Lọc theo khoảng giá tối thiểu
        public decimal? MinPrice { get; set; }
        // Lọc theo khoảng giá tối đa
        public decimal? MaxPrice { get; set; }
        // Số trang hiện tại
        public int Page { get; set; } = 1;
        // Số sản phẩm trên một trang
        public int PageSize { get; set; } = 12;

        // Danh sách sản phẩm của trang hiện tại
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        // Tổng số sản phẩm thỏa mãn điều kiện lọc
        public int TotalCount { get; set; }
        // Tổng số trang tính toán tự động
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Danh sách các danh mục phục vụ chọn trên giao diện
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        // Danh sách các thương hiệu phục vụ chọn trên giao diện
        public IEnumerable<Brand> Brands { get; set; } = new List<Brand>();
    }
}
