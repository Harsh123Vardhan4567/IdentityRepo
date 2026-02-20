namespace IdentityDemo.Services
{
    public interface IEmailService
    {
        Task SendRegistrationConfirmationEmailAsync(string toEmail, string firstName, string confirmationLink);
        Task SendAccountCreatedEmailAsync(string toEmail, string firstName, string loginLink);
        Task SendResendConfirmationEmailAsync(string toEmail, string firstName, string confirmationLink);
        Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetLink);
    }
}
