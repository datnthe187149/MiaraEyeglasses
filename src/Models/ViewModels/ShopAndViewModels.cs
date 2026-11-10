using System.ComponentModel.DataAnnotations;
using MiaraEyeglasses.Models.Entities;

namespace MiaraEyeglasses.Models.ViewModels
{
    public class PrescriptionInputModel
    {
        [Range(-15.00, 10.00, ErrorMessage = "Độ cầu mắt phải từ -15.00 đến +10.00")]
        public decimal? RightSph { get; set; }

        [Range(-6.00, 6.00, ErrorMessage = "Độ trụ mắt phải từ -6.00 đến +6.00")]
        public decimal? RightCyl { get; set; }

        [Range(0, 180, ErrorMessage = "Trục mắt phải từ 0 đến 180")]
        public short? RightAxis { get; set; }

        [Range(-15.00, 10.00, ErrorMessage = "Độ cầu mắt trái từ -15.00 đến +10.00")]
        public decimal? LeftSph { get; set; }

        [Range(-6.00, 6.00, ErrorMessage = "Độ trụ mắt trái từ -6.00 đến +6.00")]
        public decimal? LeftCyl { get; set; }

        [Range(0, 180, ErrorMessage = "Trục mắt trái từ 0 đến 180")]
        public short? LeftAxis { get; set; }

        [Range(50.0, 78.0, ErrorMessage = "Khoảng cách đồng tử PD từ 50 đến 78mm")]
        public decimal? PD { get; set; }

        public string? PrescriptionImg { get; set; }
        public string? Note { get; set; }
    }

    public class AddToCartRequest
    {
        [Required]
        public int VariantId { get; set; }

        public int LensOptionId { get; set; } = 1;

        [Range(1, 20, ErrorMessage = "Số lượng phải từ 1 đến 20")]
        public int Quantity { get; set; } = 1;

        public PrescriptionInputModel? Prescription { get; set; }

        public bool BuyNow { get; set; } = false;

        public string? CouponCode { get; set; }
    }

    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string SizeLabel { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal FramePrice { get; set; }
        public int LensOptionId { get; set; }
        public string LensName { get; set; } = string.Empty;
        public decimal LensPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsSelected { get; set; }
        public decimal LineTotal => (FramePrice + LensPrice) * Quantity;
        public PrescriptionInputModel? Prescription { get; set; }
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal Subtotal => Items.Where(i => i.IsSelected).Sum(i => i.LineTotal);
        public decimal DiscountAmount { get; set; } = 0;
        public decimal ShippingFee { get; set; } = 0;
        public decimal TotalAmount => Subtotal - DiscountAmount + ShippingFee;
        public string? AppliedCouponCode { get; set; }
    }

    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        public string RecipientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        public string ShippingAddress { get; set; } = string.Empty;

        public string? OrderNote { get; set; }

        public string? CouponCode { get; set; }

        public string PaymentMethod { get; set; } = "COD"; // Mặc định: COD (Thanh toán khi nhận hàng)

        public CartViewModel Cart { get; set; } = new();
    }

    public class ProductFilterViewModel
    {
        public string? Search { get; set; }
        public string? Sort { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string? Gender { get; set; }
        public string? Shape { get; set; }
        public string? Material { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public IEnumerable<Brand> Brands { get; set; } = new List<Brand>();
    }
}

