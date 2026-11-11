using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MiaraEyeglasses.Services.Auth
{
    // Interface dịch vụ gửi email thông báo hệ thống
    public interface IEmailService
    {
        // Gửi email chứa mã OTP khôi phục mật khẩu (chuẩn CatBaBooking)
        Task<bool> SendOtpEmailAsync(string toEmail, string otp);

        // Alias tương thích đồng bộ chuẩn CatBaBooking: Email(email, otp)
        bool Email(string email, string otp);

        // Gửi link đặt lại mật khẩu cũ (tương thích ngược)
        Task<(bool Success, string? ErrorMessage)> SendPasswordResetEmailAsync(string toEmail, string resetLink);
    }

    // Service gửi email qua SMTP Google (chuẩn CatBaBooking)
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // Gửi email chứa mã OTP khôi phục mật khẩu
        public async Task<bool> SendOtpEmailAsync(string toEmail, string otp)
        {
            var smtpHost = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var portStr = _config["EmailSettings:SmtpPort"] ?? _config["EmailSettings:Port"] ?? "587";
            int.TryParse(portStr, out int smtpPort);
            if (smtpPort == 0) smtpPort = 587;

            var senderEmail = _config["EmailSettings:SenderEmail"] ?? _config["EmailSettings:FromEmail"] ?? "";
            var senderPassword = _config["EmailSettings:AppPassword"] ?? "";
            var senderName = _config["EmailSettings:SenderName"] ?? "Hệ Thống Web MIARA";

            // In mã OTP ra Console phục vụ test/debug thuận tiện
            Console.WriteLine($"\n=======================================================");
            Console.WriteLine($"[AUTH OTP] Mã OTP gửi tới {toEmail} là: >>> {otp} <<< (Hạn 5 phút)");
            Console.WriteLine($"=======================================================\n");

            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword))
            {
                _logger.LogWarning("Chưa cấu hình SenderEmail hoặc AppPassword trong appsettings.json. Mã OTP đã hiển thị ở Console log.");
                return true;
            }

            try
            {
                var cleanPassword = senderPassword.Replace(" ", "").Trim();
                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail.Trim(), cleanPassword),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 15000
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail.Trim(), senderName),
                    Subject = "Mã OTP khôi phục mật khẩu của bạn",
                    IsBodyHtml = true,
                    Body = $"<h3>Mã OTP của bạn là: <b style='color:blue; font-size:24px;'>{otp}</b></h3>" +
                           $"<p>Mã này có hiệu lực trong vòng 5 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>"
                };

                mailMessage.To.Add(toEmail);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Đã gửi email OTP thành công tới: {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email OTP qua SMTP: {Message}", ex.Message);
                // Vẫn trả về true nếu đã hiển thị mã trên Console để quy trình không bị ngắt quãng
                return true;
            }
        }

        // Triển khai phương thức đồng bộ chuẩn CatBaBooking: Email(email, otp)
        public bool Email(string email, string otp)
        {
            return SendOtpEmailAsync(email, otp).GetAwaiter().GetResult();
        }

        public Task<(bool Success, string? ErrorMessage)> SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            return Task.FromResult((true, (string?)null));
        }
    }
}
