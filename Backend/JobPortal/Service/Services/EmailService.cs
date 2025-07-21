using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using Service.Interface;

namespace Service.Services
{
    public class EmailService:IEmailService
    {
        private readonly IConfiguration configuration;

        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task SendEmail(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(configuration["Email:SmtpHost"])
            {
                Port = int.Parse(configuration["Email:SmtpPort"]),
                Credentials = new NetworkCredential(configuration["Email:SenderEmail"], configuration["Email:SenderPassword"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(configuration["Email:SenderEmail"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
