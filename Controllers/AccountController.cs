
using IdentityDemo.Services;
using IdentityDemo.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;
        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (model.ProfileImagebase64 != null)
                {
                    var ms = new MemoryStream();
                    await model.ProfileImagebase64.CopyToAsync(ms);
                    byte[] imagesbytes = ms.ToArray();

                    // Convert to Base64 string
                    string base64String = Convert.ToBase64String(imagesbytes);
                    string imageWithPrefix = $"data:{model.ProfileImagebase64.ContentType};base64,{base64String}";
                    model.Image = imageWithPrefix;
                }

                var result = await _accountService.RegisterUserAsync(model);
                if (result.Succeeded)
                    return RedirectToAction("RegistrationConfirmation");
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", model.Email);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult RegistrationConfirmation()
        {

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(Guid userId, string token)
        {
            try
            {
                if (userId == Guid.Empty || string.IsNullOrEmpty(token))
                    return BadRequest("Invalid email confirmation request.");
                var result = await _accountService.ConfirmEmailAsync(userId, token);
                if (result.Succeeded)
                    return View("EmailConfirmed");
                // Combine errors into one message or pass errors to the view
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for UserId: {UserId}", userId);
                ModelState.AddModelError("", "An unexpected error occurred during email confirmation.");
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult Login(string? ReturnUrl = null)
        {
            ViewData["ReturnUrl"] = ReturnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);
                var result = await _accountService.LoginUserAsync(model);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction("Profile", "Account"); ;
                }
                if (result.IsNotAllowed)
                    ModelState.AddModelError("", "Email is not confirmed yet.");
                else
                    ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", model.Email);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");
            try
            {
                var model = await _accountService.GetUserProfileByEmailAsync(email);
                return View(model);
            }
            catch (ArgumentException)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _accountService.LogoutUserAsync();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                // Optionally redirect to error page or home with message
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            await _accountService.SendPasswordResetLinkAsync(model.Email);
            // Always show confirmation view (don’t reveal if email exists)
            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(ResendConfirmationEmailViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);
                await _accountService.SendEmailConfirmationAsync(model.Email);

                return RedirectToAction("RegistrationConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email confirmation to: {Email}", model.Email);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return BadRequest("Invalid password reset request.");
            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var result = await _accountService.ResetPasswordAsync(model);
            if (result.Succeeded)
                return View("ResetPasswordConfirmation");
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            // Generate the callback URL for to redirect after login
            // This URL includes the returnUrl as a route parameter, which will be used to redirect the user
            // back to the original page they were trying to access after a successful external login.
            var redirectUrl = Url.Action(
                action: "ExternalLoginCallback", // The name of the callback action method.
                controller: "Account",           // The name of the controller containing the callback method.
                values: new { ReturnUrl = returnUrl } // Pass the returnUrl as a parameter to the callback method.
            );

            // Configure authentication parameters for the external login.
            var properties = _accountService.ConfigureExternalLogin(provider, redirectUrl);

            // Redirect the user to the external provider's login page (e.g., Google or Facebook).
            // The "ChallengeResult" triggers the external authentication process, which redirects the user
            // to the external provider's login page using the configured properties.
            return new ChallengeResult(provider, properties);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            // If no returnUrl is provided, default to the application's home page.
            returnUrl = returnUrl ?? Url.Content("~/");

            // Check if an error occurred during the external authentication process.
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction("Login");
            }

            // Retrieve login information about the user from the external login provider.
            var info = await _accountService.GetExternalLoginInfoAsync();

            // If the login information could not be retrieved, display an error message
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information.");
                return RedirectToAction("Login");
            }

            // Attempt to sign in the user using their external login details.
            var result = await _accountService.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);

            // If the external login succeeds, redirect the parent window to the returnUrl
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            // If the user does not have a corresponding record in the UserLogins table create a new account
            var createResult = await _accountService.CreateExternalUserAsync(info);
            if (createResult.Succeeded)
                return LocalRedirect(returnUrl);

            foreach (var error in createResult.Errors)
                ModelState.AddModelError("", error.Description);

            return View("Error");
        }
    }
}
