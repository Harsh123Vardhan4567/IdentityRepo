
using IdentityDemo.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.Services
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterViewModel model);
        Task<IdentityResult> ConfirmEmailAsync(Guid userId, string token);
        Task<SignInResult> LoginUserAsync(LoginViewModel model);
        Task LogoutUserAsync();
        Task SendEmailConfirmationAsync(string email);
        Task<ProfileViewModel> GetUserProfileByEmailAsync(string email);
        Task<bool> SendPasswordResetLinkAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model);
    }
}
