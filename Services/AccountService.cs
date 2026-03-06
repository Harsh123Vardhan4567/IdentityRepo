
using IdentityDemo.Helper;
using IdentityDemo.Models;
using IdentityDemo.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
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
            if (roleres.Succeeded == false) { return roleres; }

            // 🔹 Add Claims
            var claims = new List<Claim>
            {
                new Claim("Permission", "ViewUsers"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var claimResult = await _userManager.AddClaimsAsync(user, claims);
            if (!claimResult.Succeeded)
            {
                return claimResult;
            }


            var token = GenerateToken.GenerateEmailConfirmationTokenAsync(_userManager, user);
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is not configured.");
            var confirmationLink = $"{baseUrl}/Account/ConfirmEmail?userId={user.Id}&token={token.Result}";
            await _emailService.SendRegistrationConfirmationEmailAsync(user.Email, user.FirstName, confirmationLink);
            return userres;


        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            // Find the user associated with the provided email
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Invalid request." });

            }
            // Decode the token that was passed in from the reset link
            var decodebytes = WebEncoders.Base64UrlDecode(model.Token);
            var decodeToken = Encoding.UTF8.GetString(decodebytes);

            var result = await _userManager.ResetPasswordAsync(user, decodeToken, model.ConfirmPassword);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
            }
            return result;

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

        public async Task<bool> SendPasswordResetLinkAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null && !(user.EmailConfirmed))
                return false;


            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var baseUrl = _configuration["AppSettings:BaseUrl"]
                              ?? throw new InvalidOperationException("BaseUrl is not configured.");

            var confirmationLink = $"{baseUrl}/Account/ResetPassword?email={user.Email}&token={encodedToken}";

            await _emailService.SendResendConfirmationEmailAsync(
                user.Email!,
                user.FirstName ?? "User",
                confirmationLink
            );
            return true;
        }

           public AuthenticationProperties ConfigureExternalLogin(string provider, string? redirectUrl)
        {
            // The ConfigureExternalAuthenticationProperties method sets up parameters needed for the external provider,
            // such as the login provider name (e.g., Google, Facebook) and the redirect URL to be used after login.
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }

        public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync()
        {
            // Retrieve login information about the user from the external login provider (e.g., Google, Facebook).
            // This includes details like the provider's name and the user's identifier within that provider.
            return await _signInManager.GetExternalLoginInfoAsync();
        }

        public async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent)
        {
            // Attempt to sign in the user using their external login details.
            // If a corresponding record exists in the UserLogins table, the user will be logged in.
            return await _signInManager.ExternalLoginSignInAsync(
                loginProvider,    // The name of the external login provider (e.g., Google, Facebook).
                providerKey,      // The unique identifier of the user within the external provider.
                isPersistent: isPersistent,   // Indicates whether the login session should persist across browser restarts.
                bypassTwoFactor: true  // Bypass two-factor authentication if enabled.
            );
        }

        public async Task<IdentityResult> CreateExternalUserAsync(ExternalLoginInfo info)
        {
            // Extract email claim (mandatory for identifying the user)
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return IdentityResult.Failed(new IdentityError { Description = "Email not received from external provider." });

            // Check if user with this email already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                // If user already exists, link this external login to the existing account
                var loginResult = await _userManager.AddLoginAsync(existingUser, info);

                if (loginResult.Succeeded)
                {
                    // Update last login time
                    existingUser.LastLogin = DateTime.UtcNow;
                    await _userManager.UpdateAsync(existingUser);

                    // Sign in the existing user
                    await _signInManager.SignInAsync(existingUser, isPersistent: false);
                }

                return loginResult;
            }

            // Extract optional claims
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

            // Create new ApplicationUser instance
            var user = new ApplicationUser
            {
                UserName = email,                     // Use email as username (unique in system)
                Email = email,                             // Primary email from Google
                FirstName = firstName,              // From Google claim (or blank if missing)
                LastName = lastName,               // From Google claim (nullable)
                EmailConfirmed = true,              // External providers already confirm email
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };

            // Create the user in Identity DB (Users table)
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                return result;

            // Link external login info (UserLogins table)
            result = await _userManager.AddLoginAsync(user, info);

            // Sign in immediately if successful
            if (result.Succeeded)
                await _signInManager.SignInAsync(user, isPersistent: false);

            return result;
        }


    }
 }

