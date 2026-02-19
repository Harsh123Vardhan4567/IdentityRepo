
using IdentityDemo.Helper;
using IdentityDemo.Models;
using IdentityDemo.ViewModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace IdentityDemo.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _configuration = configuration;
        }
        public async  Task<IdentityResult> ConfirmEmailAsync(Guid userId, string token)
        {
            if (userId == Guid.Empty || string.IsNullOrEmpty(token))
                return IdentityResult.Failed(new IdentityError { Description = "Invalid token or user ID." });
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            if (user.EmailConfirmed)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Alredy confirmed." });
            }
            
            var decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is not configured.");
                var loginLink = $"{baseUrl}/Account/Login";
                await _emailService.SendAccountCreatedEmailAsync(user.Email!, user.FirstName!, loginLink);
            }
            return result;
        }

        public async  Task<ProfileViewModel> GetUserProfileByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                
                 throw new ArgumentException("User not found.", nameof(email));
                return new ProfileViewModel()
                {
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName!,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    ProfileImageBase64=user.ProfileImage,
                    LastLoggedIn = user.LastLogin,
                    CreatedOn = user.CreatedOn,
                    DateOfBirth = user.DateOfBirth
                };
           
        }


        public  async Task<SignInResult> LoginUserAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return SignInResult.Failed;
            }
            if (!await _userManager.IsEmailConfirmedAsync(user))
                return SignInResult.NotAllowed;


           
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
              

            }
            return result;
        }
           

        

        public async  Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();    
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterViewModel model)
        {
            var user = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ProfileImage = model.Image,
                DateOfBirth = model.DateOfBirth,
                IsActive = true,
                PhoneNumber = model.PhoneNumber,
                CreatedOn = DateTime.UtcNow,

            };
            IdentityResult userres = await _userManager.CreateAsync(user, model.Password);
            if (userres.Succeeded==false)
            {
                return userres;
            }
            
            IdentityResult roleres = await _userManager.AddToRoleAsync(user, "User");
            if (roleres.Succeeded==false) { return roleres; }

            var token = GenerateToken.GenerateEmailConfirmationTokenAsync(_userManager, user);
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is not configured.");
            var confirmationLink = $"{baseUrl}/Account/ConfirmEmail?userId={user.Id}&token={token.Result}";
            await _emailService.SendRegistrationConfirmationEmailAsync(user.Email, user.FirstName, confirmationLink);
            return userres;


        }

        public async Task SendEmailConfirmationAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new InvalidOperationException("User not found.");

            if (user.EmailConfirmed)
                return; 

           
            var token = GenerateToken.GenerateEmailConfirmationTokenAsync(_userManager,user);

            var baseUrl = _configuration["AppSettings:BaseUrl"]
                          ?? throw new InvalidOperationException("BaseUrl is not configured.");

            var confirmationLink = $"{baseUrl}/Account/ConfirmEmail?userId={user.Id}&token={token.Result}";

            await _emailService.SendResendConfirmationEmailAsync(
                user.Email!,
                user.FirstName ?? "User",
                confirmationLink
            );
        }

    }
}
