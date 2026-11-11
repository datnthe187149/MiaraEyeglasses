using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MiaraEyeglasses.Models.Entities
{
    // Model mapping View CSDL: vw_DailyRevenue (Báo cáo doanh thu theo ngày)
    [Keyless]
    // Class định nghĩa: DailyRevenueReport
    public class DailyRevenueReport
    {
        // Thuộc tính: RevenueDate
        public DateOnly RevenueDate { get; set; }
        // Thuộc tính: OrderCount
        public int OrderCount { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        // Thuộc tính: Revenue
        public decimal Revenue { get; set; }
    }

    // Model mapping View CSDL: vw_BestSellers (Báo cáo sản phẩm bán chạy nhất)
    [Keyless]
    // Class định nghĩa: BestSellerReport
    public class BestSellerReport
    {
        // Thuộc tính: ProductId
        public int ProductId { get; set; }
        // Thuộc tính: ProductName
        public string ProductName { get; set; } = string.Empty;
        // Thuộc tính: SoldQty
        public int SoldQty { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        // Thuộc tính: Revenue
        public decimal Revenue { get; set; }
    }

    // Model mapping View CSDL: vw_LowStock (Báo cáo biến thể sắp hết hàng <= 5)
    [Keyless]
    // Class định nghĩa: LowStockReport
    public class LowStockReport
    {
        // Thuộc tính: ProductId
        public int ProductId { get; set; }
        // Thuộc tính: ProductName
        public string ProductName { get; set; } = string.Empty;
        // Thuộc tính: VariantId
        public int VariantId { get; set; }
        // Thuộc tính: Sku
        public string Sku { get; set; } = string.Empty;
        // Thuộc tính: ColorName
        public string ColorName { get; set; } = string.Empty;
        // Thuộc tính: SizeLabel
        public string SizeLabel { get; set; } = string.Empty;
        // Thuộc tính: StockQty
        public int StockQty { get; set; }
    }
}

