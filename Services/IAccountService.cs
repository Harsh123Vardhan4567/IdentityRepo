
using IdentityDemo.ViewModel;
using Microsoft.AspNetCore.Authentication;
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

        //New Methods for External Login
        AuthenticationProperties ConfigureExternalLogin(string provider, string? redirectUrl);
        Task<ExternalLoginInfo?> GetExternalLoginInfoAsync();
        Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent);
        Task<IdentityResult> CreateExternalUserAsync(ExternalLoginInfo info);
    }
}
