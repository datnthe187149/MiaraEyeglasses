using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.Order;

namespace MiaraEyeglasses.Services.Order
{
    // Giao diện quản lý toàn bộ nghiệp vụ đơn hàng (Business Logic Layer)
    public interface IOrderService
    {
        // Tạo mới đơn hàng khi khách bấm Đặt hàng
        Task<(bool Success, string? ErrorMessage, int OrderId)> CreateOrderAsync(int userId, CheckoutViewModel checkout);

        // Lấy chi tiết đơn hàng (có kiểm tra quyền sở hữu của khách)
        Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId, int? userId = null);

        // Lấy danh sách lịch sử đơn hàng của một người dùng
        Task<IEnumerable<Models.Entities.Order>> GetOrdersByUserAsync(int userId);

        // Lấy danh sách toàn bộ đơn hàng cho Nhân viên và Quản trị viên
        Task<IEnumerable<Models.Entities.Order>> GetAllOrdersForStaffAsync(byte? status = null);

        // Hủy đơn hàng và hoàn lại số lượng tồn kho sản phẩm
        Task<(bool Success, string? ErrorMessage)> CancelOrderAsync(int orderId, int? userId, string reason);

        // Cập nhật trạng thái đơn hàng (Dành cho Nhân viên)
        Task<(bool Success, string? ErrorMessage)> UpdateOrderStatusAsync(int orderId, byte newStatus, int staffId);

        // Hoàn tiền đơn hàng (Dành cho Quản trị viên Admin)
        Task<(bool Success, string? ErrorMessage)> RefundOrderAsync(int orderId, int adminId, string refundTxnNo, string reason, decimal? refundAmount = null);

        // Lưu bản ghi phương thức thanh toán cho đơn hàng (COD / DIRECT / VNPAY)
        Task AddPaymentRecordAsync(int orderId, string method, decimal amount, byte status, string txnRef);
    }

    // Triển khai nghiệp vụ đơn hàng
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // Tạo đơn hàng mới từ giỏ hàng và thông tin thanh toán
        public async Task<(bool Success, string? ErrorMessage, int OrderId)> CreateOrderAsync(int userId, CheckoutViewModel checkout)
        {
            try
            {
                // Gọi Stored Procedure để thực hiện tạo đơn hàng trong 1 Transaction an toàn:
                // - Lưu Order và OrderDetails
                // - Trừ số lượng tồn kho sản phẩm
                // - Xóa các món đã đặt khỏi giỏ hàng
                int orderId = await _orderRepository.CreateOrderViaProcedureAsync(
                    userId: userId,
                    recipientName: checkout.RecipientName,
                    phone: checkout.Phone,
                    address: checkout.ShippingAddress,
                    note: checkout.OrderNote,
                    couponCode: checkout.CouponCode,
                    shippingFee: checkout.Cart.ShippingFee,
                    windowMinutes: 15
                );

                return (true, null, orderId);
            }
            catch (Exception ex)
            {
                // Bắt lỗi nếu có ngoại lệ xảy ra (ví dụ hết hàng hoặc lỗi kết nối DB)
                return (false, ex.Message, 0);
            }
        }

        // Xem chi tiết đơn hàng
        public async Task<Models.Entities.Order?> GetOrderDetailsAsync(int orderId, int? userId = null)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order == null)
            {
                return null;
            }

            // Nếu truyền userId của khách hàng thì kiểm tra xem đơn hàng này có đúng là của khách đó không
            if (userId.HasValue)
            {
                if (order.UserId != userId.Value)
                {
                    return null; // Khách hàng không được xem đơn của người khác
                }
            }

            return order;
        }

        // Lấy danh sách đơn hàng của khách hàng theo userId
        public async Task<IEnumerable<Models.Entities.Order>> GetOrdersByUserAsync(int userId)
        {
            return await _orderRepository.GetOrdersByUserIdAsync(userId);
        }

        // Lấy danh sách toàn bộ đơn hàng trong hệ thống cho Staff / Admin
        public async Task<IEnumerable<Models.Entities.Order>> GetAllOrdersForStaffAsync(byte? status = null)
        {
            return await _orderRepository.GetAllOrdersForAdminAsync(status);
        }

        // Khách hàng hoặc Nhân viên hủy đơn hàng
        public async Task<(bool Success, string? ErrorMessage)> CancelOrderAsync(int orderId, int? userId, string reason)
        {
            try
            {
                // Gọi Stored Procedure hủy đơn: hoàn lại tồn kho cho từng sản phẩm
                await _orderRepository.CancelOrderViaProcedureAsync(orderId, userId, reason);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Nhân viên đổi trạng thái đơn hàng (Ví dụ: Chờ xác nhận -> Đã xác nhận -> Đang giao)
        public async Task<(bool Success, string? ErrorMessage)> UpdateOrderStatusAsync(int orderId, byte newStatus, int staffId)
        {
            try
            {
                await _orderRepository.UpdateOrderStatusViaProcedureAsync(orderId, newStatus, staffId);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Quản trị viên thực hiện hoàn tiền đơn hàng
        public async Task<(bool Success, string? ErrorMessage)> RefundOrderAsync(int orderId, int adminId, string refundTxnNo, string reason, decimal? refundAmount = null)
        {
            try
            {
                await _orderRepository.RefundOrderViaProcedureAsync(orderId, adminId, refundTxnNo, reason, refundAmount);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task AddPaymentRecordAsync(int orderId, string method, decimal amount, byte status, string txnRef)
        {
            await _orderRepository.AddPaymentRecordAsync(orderId, method, amount, status, txnRef);
        }
    }
}
