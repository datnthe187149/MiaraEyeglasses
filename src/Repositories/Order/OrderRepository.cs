using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.Common;

namespace MiaraEyeglasses.Repositories.Order
{
    public interface IOrderRepository : IBaseRepository<Models.Entities.Order>
    {
        Task<Models.Entities.Order?> GetOrderWithDetailsAsync(int orderId);
        Task<Models.Entities.Order?> GetOrderByCodeAsync(string orderCode);
        Task<IEnumerable<Models.Entities.Order>> GetOrdersByUserIdAsync(int userId);
        Task<IEnumerable<Models.Entities.Order>> GetAllOrdersForAdminAsync(byte? status = null);
        Task<(IEnumerable<Models.Entities.Order> Orders, int TotalCount)> GetOrdersPagedForAdminAsync(
            int page, int pageSize, byte? status = null, string? search = null);
        Task<int> CreateOrderViaProcedureAsync(
            int userId, string recipientName, string phone, string address,
            string? note, string? couponCode, decimal shippingFee, int windowMinutes = 15);
        Task<string> ConfirmVnPayPaymentViaProcedureAsync(
            string txnRef, string responseCode, decimal amount,
            string? txnNo = null, string? bankCode = null, DateTime? payDate = null, string? rawResponse = null);
        Task CancelOrderViaProcedureAsync(int orderId, int? userId, string reason);
        Task RefundOrderViaProcedureAsync(int orderId, int adminId, string refundTxnNo, string reason, decimal? refundAmount = null);
        Task UpdateOrderStatusViaProcedureAsync(int orderId, byte newStatus, int staffId);
    }

    public class OrderRepository : BaseRepository<Models.Entities.Order>, IOrderRepository
    {
        public OrderRepository(EyeglassesDbContext context) : base(context) { }

        public async Task<Models.Entities.Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _dbSet.Include(o => o.Items)
                               .Include(o => o.OrderStatus)
                               .Include(o => o.Payments)
                               .Include(o => o.StatusHistories)
                               .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<Models.Entities.Order?> GetOrderByCodeAsync(string orderCode)
        {
            return await _dbSet.Include(o => o.Items)
                               .Include(o => o.OrderStatus)
                               .Include(o => o.Payments)
                               .Include(o => o.StatusHistories)
                               .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        }

        public async Task<IEnumerable<Models.Entities.Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _dbSet.Include(o => o.OrderStatus)
                               .Include(o => o.Items)
                               .Where(o => o.UserId == userId)
                               .OrderByDescending(o => o.CreatedAt)
                               .ToListAsync();
        }

        public async Task<IEnumerable<Models.Entities.Order>> GetAllOrdersForAdminAsync(byte? status = null)
        {
            var query = _dbSet.Include(o => o.User)
                              .Include(o => o.OrderStatus)
                              .Include(o => o.Items)
                              .AsQueryable();

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        }

        public async Task<(IEnumerable<Models.Entities.Order> Orders, int TotalCount)> GetOrdersPagedForAdminAsync(
            int page, int pageSize, byte? status = null, string? search = null)
        {
            var query = _dbSet.Include(o => o.User)
                              .Include(o => o.OrderStatus)
                              .Include(o => o.Items)
                              .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(o => o.OrderCode.ToLower().Contains(term)
                                      || o.RecipientName.ToLower().Contains(term)
                                      || o.Phone.Contains(term));
            }

            int totalCount = await query.CountAsync();
            var orders = await query.OrderByDescending(o => o.CreatedAt)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<int> CreateOrderViaProcedureAsync(
            int userId, string recipientName, string phone, string address,
            string? note, string? couponCode, decimal shippingFee, int windowMinutes = 15)
        {
            using var connection = new SqlConnection(_context.Database.GetConnectionString());
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@RecipientName", recipientName);
            parameters.Add("@Phone", phone);
            parameters.Add("@ShippingAddress", address);
            parameters.Add("@OrderNote", note);
            parameters.Add("@CouponCode", string.IsNullOrWhiteSpace(couponCode) ? null : couponCode);
            parameters.Add("@ShippingFee", shippingFee);
            parameters.Add("@PaymentWindowMinutes", windowMinutes);
            parameters.Add("@OrderId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("dbo.sp_CreateOrderFromCart", parameters, commandType: CommandType.StoredProcedure);
            return parameters.Get<int>("@OrderId");
        }

        public async Task<string> ConfirmVnPayPaymentViaProcedureAsync(
            string txnRef, string responseCode, decimal amount,
            string? txnNo = null, string? bankCode = null, DateTime? payDate = null, string? rawResponse = null)
        {
            using var connection = new SqlConnection(_context.Database.GetConnectionString());
            var parameters = new DynamicParameters();
            parameters.Add("@TransactionRef", txnRef);
            parameters.Add("@ResponseCode", responseCode);
            parameters.Add("@Amount", amount);
            parameters.Add("@TransactionNo", txnNo);
            parameters.Add("@BankCode", bankCode);
            parameters.Add("@PayDate", payDate);
            parameters.Add("@RawResponse", rawResponse);

            var result = await connection.QueryFirstOrDefaultAsync<string>(
                "dbo.sp_ConfirmVnPayPayment", parameters, commandType: CommandType.StoredProcedure);

            return result ?? "NOT_FOUND";
        }

        public async Task CancelOrderViaProcedureAsync(int orderId, int? userId, string reason)
        {
            using var connection = new SqlConnection(_context.Database.GetConnectionString());
            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId);
            parameters.Add("@UserId", userId);
            parameters.Add("@Reason", reason);

            await connection.ExecuteAsync("dbo.sp_CancelOrder", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task RefundOrderViaProcedureAsync(int orderId, int adminId, string refundTxnNo, string reason, decimal? refundAmount = null)
        {
            using var connection = new SqlConnection(_context.Database.GetConnectionString());
            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId);
            parameters.Add("@AdminId", adminId);
            parameters.Add("@RefundTransactionNo", refundTxnNo);
            parameters.Add("@Reason", reason);
            parameters.Add("@RefundAmount", refundAmount);

            await connection.ExecuteAsync("dbo.sp_RefundOrder", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task UpdateOrderStatusViaProcedureAsync(int orderId, byte newStatus, int staffId)
        {
            using var connection = new SqlConnection(_context.Database.GetConnectionString());
            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId);
            parameters.Add("@NewStatus", newStatus);
            parameters.Add("@StaffId", staffId);

            await connection.ExecuteAsync("dbo.sp_UpdateOrderStatus", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}

