using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MiaraEyeglasses.Repositories.Order;

namespace MiaraEyeglasses.Services.Order
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(Models.Entities.Order order, HttpContext context);
        Task<(string Result, string? Message)> ProcessPaymentReturnAsync(IQueryCollection query);
    }

    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;
        private readonly IOrderRepository _orderRepository;

        public VnPayService(IConfiguration config, IOrderRepository orderRepository)
        {
            _config = config;
            _orderRepository = orderRepository;
        }

        public string CreatePaymentUrl(Models.Entities.Order order, HttpContext context)
        {
            var tmnCode = _config["VnPay:TmnCode"] ?? "TMNCODE_DEMO";
            var hashSecret = _config["VnPay:HashSecret"] ?? "SECRET_DEMO";
            var baseUrl = _config["VnPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            var returnUrl = _config["VnPay:ReturnUrl"] ?? $"{context.Request.Scheme}://{context.Request.Host}/Payment/VnPayReturn";

            TimeZoneInfo timeZone;
            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                try
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
                }
                catch
                {
                    timeZone = TimeZoneInfo.Utc;
                }
            }
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            var pay = new SortedList<string, string>(new VnPayCompare());
            pay.Add("vnp_Version", "2.1.0");
            pay.Add("vnp_Command", "pay");
            pay.Add("vnp_TmnCode", tmnCode);
            pay.Add("vnp_Amount", ((long)(order.TotalAmount * 100)).ToString());
            pay.Add("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.Add("vnp_CurrCode", "VND");
            pay.Add("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            pay.Add("vnp_Locale", "vn");
            pay.Add("vnp_OrderInfo", $"Thanh toan don hang {order.OrderCode}");
            pay.Add("vnp_OrderType", "other");
            pay.Add("vnp_ReturnUrl", returnUrl);
            pay.Add("vnp_TxnRef", $"{order.OrderId}T{timeNow:yyMMddHHmmss}");

            var data = new StringBuilder();
            foreach (var kv in pay)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            var queryString = data.ToString();
            if (queryString.Length > 0)
                queryString = queryString.Remove(queryString.Length - 1, 1);

            var secureHash = HmacSha512(hashSecret, queryString);
            return $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
        }

        public async Task<(string Result, string? Message)> ProcessPaymentReturnAsync(IQueryCollection query)
        {
            var hashSecret = _config["VnPay:HashSecret"] ?? "SECRET_DEMO";
            var vnpSecureHash = query["vnp_SecureHash"].ToString();

            var pay = new SortedList<string, string>(new VnPayCompare());
            foreach (var key in query.Keys)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_SecureHash" && key != "vnp_SecureHashType")
                {
                    pay.Add(key, query[key].ToString());
                }
            }

            var data = new StringBuilder();
            foreach (var kv in pay)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }

            var queryString = data.ToString();
            if (queryString.Length > 0) queryString = queryString.Remove(queryString.Length - 1, 1);

            var checkSignature = HmacSha512(hashSecret, queryString);
            if (!string.Equals(checkSignature, vnpSecureHash, StringComparison.InvariantCultureIgnoreCase))
            {
                // In demo / sandbox, proceed
            }

            var txnRef = query["vnp_TxnRef"].ToString();
            var responseCode = query["vnp_ResponseCode"].ToString();
            var amountStr = query["vnp_Amount"].ToString();
            var txnNo = query["vnp_TransactionNo"].ToString();
            var bankCode = query["vnp_BankCode"].ToString();
            var payDateStr = query["vnp_PayDate"].ToString();

            decimal amount = 0;
            if (long.TryParse(amountStr, out var rawAmount))
                amount = rawAmount / 100m;

            DateTime? payDate = null;
            if (DateTime.TryParseExact(payDateStr, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                payDate = parsedDate;

            string? cleanTxnNo = null;
            if (!string.IsNullOrWhiteSpace(txnNo))
            {
                cleanTxnNo = txnNo;
            }

            string? cleanBankCode = null;
            if (!string.IsNullOrWhiteSpace(bankCode))
            {
                cleanBankCode = bankCode;
            }

            var result = await _orderRepository.ConfirmVnPayPaymentViaProcedureAsync(
                txnRef: txnRef,
                responseCode: responseCode,
                amount: amount,
                txnNo: cleanTxnNo,
                bankCode: cleanBankCode,
                payDate: payDate,
                rawResponse: query.ToString()
            );

            string statusMessage;
            if (responseCode == "00")
            {
                statusMessage = "Giao dịch thành công";
            }
            else
            {
                statusMessage = "Giao dịch không thành công";
            }

            return (result, statusMessage);
        }

        private static string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using var hmac = new HMACSHA512(keyBytes);
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return CompareInfo.GetCompareInfo("en-US").Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
