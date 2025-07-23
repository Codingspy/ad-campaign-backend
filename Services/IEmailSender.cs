using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AdCampaignMVP.Models;
using Microsoft.Extensions.Configuration;

namespace AdCampaignMVP.Services
{
    public interface IEmailSender<ApplicationUser>
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }

    public class EmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var smtpClient = new SmtpClient(_config["Smtp:Host"])
            {
                Port = int.TryParse(_config["Smtp:Port"], out var port) ? port : throw new ArgumentException("SMTP 'Port' is not configured or invalid."),
                Credentials = new NetworkCredential(
                    _config["Smtp:Username"],
                    _config["Smtp:Password"]),
                EnableSsl = true,
            };

            var fromAddress = _config["Smtp:From"];
            if (string.IsNullOrWhiteSpace(fromAddress))
            {
                throw new ArgumentException("SMTP 'From' address is not configured.");
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = subject,
                Body = message,
                IsBodyHtml = false,
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
