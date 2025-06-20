using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ShiftWiseAI.Server.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendShiftScheduleEmailAsync(string toEmail, string subject, string bodyText, byte[] pdfBytes)
        {
            var message = new MimeMessage();
            var fromEmail = _config["Smtp:FromEmail"];
            var fromName = _config["Smtp:FromName"];
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { TextBody = bodyText };
            builder.Attachments.Add("ShiftSchedule.pdf", pdfBytes, new ContentType("application", "pdf"));
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            var secureOption = Enum.Parse<SecureSocketOptions>(_config["Smtp:SecureSocketOption"]);
            await client.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), secureOption);
            await client.AuthenticateAsync(_config["Smtp:Username"], _config["Smtp:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
