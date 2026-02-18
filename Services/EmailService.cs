
using IdentityDemo.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;

namespace IdentityDemo.Services
{
    public class EmailService: IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly EmailSettings _settings;
        public EmailService(IConfiguration configuration, IWebHostEnvironment env,IOptions<EmailSettings> options)
        {
            _configuration = configuration;
            _env = env;
            _settings = options.Value;
        }


        public async Task SendAccountCreatedEmailAsync(string toEmail, string firstName, string loginLink)
        {
            string filepath = Path.Combine(_env.ContentRootPath, "Templates", "AccountCreation");
            string htmlContent = await File.ReadAllTextAsync(filepath);
            htmlContent = htmlContent.Replace("{{FirstName}}", firstName);
            htmlContent = htmlContent.Replace("{{LoginLink}}", loginLink);
            htmlContent = htmlContent.Replace("{{Year}}", DateTime.UtcNow.ToString());
            await SendEmailAsync(toEmail, "Account Confirmation", htmlContent, true);

        }

        public async Task SendRegistrationConfirmationEmailAsync(string toEmail,string firstName,string confirmationLink)
        {
            try
            {
                string filepath = Path.Combine(
                    _env.ContentRootPath,
                    "Templates",
                    "RegistrationConfirmation.html");  // ⚠ add extension

                if (!File.Exists(filepath))
                    throw new FileNotFoundException("Email template not found.", filepath);

                string htmlcontent = await File.ReadAllTextAsync(filepath);

                htmlcontent = htmlcontent.Replace("{{firstName}}", firstName);
                htmlcontent = htmlcontent.Replace("{{confirmationLink}}", confirmationLink);
                htmlcontent = htmlcontent.Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

                await SendEmailAsync(toEmail, "Email Confirmation", htmlcontent, true);
            }
            catch (Exception ex)
            {
                // Log the error properly
                Console.WriteLine($"Email sending failed: {ex.Message}");

                // Optional: rethrow if you want calling method to know
                throw;
            }
        }


        public async  Task SendResendConfirmationEmailAsync(string toEmail, string firstName, string confirmationLink)
        {
            string filepath = Path.Combine(_env.ContentRootPath, "Templates", "RegistrationConfirmation");
            string htmlcontent = await File.ReadAllTextAsync(filepath);
            htmlcontent = htmlcontent.Replace("{{firstName}}", firstName);
            htmlcontent = htmlcontent.Replace("{{ConfirmationLink}}", confirmationLink);
            htmlcontent = htmlcontent.Replace("{{DateTime}} ", DateTime.UtcNow.Year.ToString());

            await SendEmailAsync(toEmail, "Email Confirmation", htmlcontent, true);
        }
        private async Task SendEmailAsync(string toEmail, string subject, string body, bool isBodyHtml = false)
        {
            try
            {
                var smtpServer = _settings.SmtpServer;
                var smtpPort = _settings.Port;
                var senderEmail =_settings.SenderEmail;
                var senderName =_settings.SenderName;
                var password = _settings.Password;
               
                using var message = new MailMessage
                {
                    From = new MailAddress(senderEmail!, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isBodyHtml
                };
                message.To.Add(new MailAddress(toEmail));

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true
                };
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


    }
}
