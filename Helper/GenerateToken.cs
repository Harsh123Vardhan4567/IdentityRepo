using IdentityDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace IdentityDemo.Helper
{
    public static  class GenerateToken
    {
        public static async Task<string> GenerateEmailConfirmationTokenAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            return WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(token)
            );
        }

    }
}
