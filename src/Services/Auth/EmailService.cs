using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MiaraEyeglasses.Services.Auth
{
    public interface IEmailService
    {
        Task<(bool Success, string? ErrorMessage)> SendPasswordResetEmailAsync(string toEmail, string resetLink);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<(bool Success, string? ErrorMessage)> SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var smtpHost = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var smtpPortStr = _config["EmailSettings:SmtpPort"] ?? "587";
            int.TryParse(smtpPortStr, out int smtpPort);
            if (smtpPort == 0) smtpPort = 587;

            var senderEmail = _config["EmailSettings:SenderEmail"] ?? "";
            var senderPassword = _config["EmailSettings:AppPassword"] ?? "";
            var senderName = _config["EmailSettings:SenderName"] ?? "MIARA Eyeglasses";

            if (string.IsNullOrWhiteSpace(senderEmail) || senderEmail.Contains("YOUR_GMAIL") || string.IsNullOrWhiteSpace(senderPassword))
            {
                var msg = "EmailSettings:SenderEmail hoặc AppPassword chưa được cấu hình địa chỉ Gmail thực tế trong appsettings.json.";
                _logger.LogWarning(msg);
                return (false, msg);
            }

            try
            {
                var cleanPassword = senderPassword.Replace(" ", "").Trim();
                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail.Trim(), cleanPassword),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 15000
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail.Trim(), senderName),
                    Subject = "[MIARA Eyeglasses] Yêu cầu đặt lại mật khẩu của bạn",
                    IsBodyHtml = true,
                    Body = $@"
<div style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; max-width: 580px; margin: 0 auto; padding: 32px 24px; background: #ffffff; border: 1px solid #e2e8f0; border-radius: 12px;"">
    <div style=""text-align: center; margin-bottom: 24px;"">
        <h1 style=""color: #286598; font-size: 24px; margin: 0; font-weight: 700; letter-spacing: 1px;"">MIARA EYEGLASSES</h1>
        <p style=""color: #64748b; font-size: 13px; margin: 4px 0 0 0;"">See yourself from a new angle</p>
    </div>
    
    <div style=""background: #f8fafc; border-radius: 8px; padding: 20px; margin-bottom: 24px; border: 1px solid #e2e8f0;"">
        <h2 style=""font-size: 17px; color: #1e293b; margin: 0 0 12px 0;"">Yêu cầu đặt lại mật khẩu</h2>
        <p style=""font-size: 14px; color: #334155; line-height: 1.6; margin: 0 0 16px 0;"">
            Xin chào, chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản <strong>{toEmail}</strong> trên hệ thống MIARA Eyeglasses.
        </p>
        <p style=""font-size: 14px; color: #334155; line-height: 1.6; margin: 0 0 20px 0;"">
            Vui lòng bấm vào nút bên dưới để tạo mật khẩu mới. Liên kết này có thời hạn trong <strong>30 phút</strong>:
        </p>
        <div style=""text-align: center; margin: 24px 0;"">
            <a href=""{resetLink}"" style=""display: inline-block; background-color: #286598; color: #ffffff; padding: 12px 32px; font-size: 15px; font-weight: 600; text-decoration: none; border-radius: 8px; box-shadow: 0 2px 4px rgba(40,101,152,0.25);"">
                Đặt lại mật khẩu
            </a>
        </div>
        <p style=""font-size: 12px; color: #64748b; margin: 0;"">
            Nếu nút bấm trên không phản hồi, bạn có thể dán đường link sau vào trình duyệt:
        </p>
        <p style=""font-size: 12px; color: #2563eb; word-break: break-all; margin: 4px 0 0 0;"">
            <a href=""{resetLink}"" style=""color: #2563eb;"">{resetLink}</a>
        </p>
    </div>

    <p style=""font-size: 12px; color: #94a3b8; line-height: 1.5; margin: 0; text-align: center;"">
        Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email. Mật khẩu của bạn vẫn an toàn.<br />
        © 2026 MIARA Eyeglasses. All rights reserved.
    </p>
</div>"
                };

                mailMessage.To.Add(toEmail);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Đã gửi email đặt lại mật khẩu thành công tới: {Email}", toEmail);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email qua SMTP: {Message}", ex.Message);
                return (false, ex.Message);
            }
        }
    }
}

