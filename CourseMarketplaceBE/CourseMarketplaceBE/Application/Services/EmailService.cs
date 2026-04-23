using CourseMarketplaceBE.Application.IServices;
using System.Net;
using System.Net.Mail;


namespace CourseMarketplaceBE.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var host = _config["EmailSettings:Host"];
            var portStr = _config["EmailSettings:Port"];
            var enableSslStr = _config["EmailSettings:EnableSSL"];
            var email = _config["EmailSettings:Email"];
            var password = _config["EmailSettings:Password"];

            //  CHECK NULL RÕ RÀNG (đỡ debug mù)
            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portStr) ||
                string.IsNullOrWhiteSpace(enableSslStr) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("EmailSettings is missing in configuration (.env or docker)");
            }

            if (!int.TryParse(portStr, out var port))
                throw new Exception("EMAIL_PORT is invalid");

            if (!bool.TryParse(enableSslStr, out var enableSsl))
                throw new Exception("EMAIL_ENABLESSL is invalid");

            var smtpClient = new SmtpClient(host)
            {
                Port = port,
                Credentials = new NetworkCredential(email, password),
                EnableSsl = enableSsl,
            };

            var mail = new MailMessage
            {
                From = new MailAddress(email),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(to);

            await smtpClient.SendMailAsync(mail);
        }
    }
}
