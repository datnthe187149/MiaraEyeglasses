using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MiaraEyeglasses.Models.Entities
{
    /// <summary>
    /// Model mapping View CSDL: vw_DailyRevenue (Báo cáo doanh thu theo ngày)
    /// </summary>
    [Keyless]
    public class DailyRevenueReport
    {
        public DateOnly RevenueDate { get; set; }
        public int OrderCount { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Model mapping View CSDL: vw_BestSellers (Báo cáo sản phẩm bán chạy nhất)
    /// </summary>
    [Keyless]
    public class BestSellerReport
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int SoldQty { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Model mapping View CSDL: vw_LowStock (Báo cáo biến thể sắp hết hàng <= 5)
    /// </summary>
    [Keyless]
    public class LowStockReport
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int VariantId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string SizeLabel { get; set; } = string.Empty;
        public int StockQty { get; set; }
    }
}

