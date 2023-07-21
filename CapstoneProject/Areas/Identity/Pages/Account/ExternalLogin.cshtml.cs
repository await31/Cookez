using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Firebase.Auth;
using Firebase.Storage;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using BusinessObjects.Models;

namespace CapstoneProject.Areas.Identity.Pages.Account {
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel {
        private readonly SignInManager<BusinessObjects.Models.Account> _signInManager;
        private readonly UserManager<BusinessObjects.Models.Account> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly RecipeOrganizerContext _context;

        public static string ApiKey = "AIzaSyDIXdDdvo8NguMgxLvn4DWMNS-vXkUxoag";
        public static string Bucket = "cookez-cloud.appspot.com";
        public static string AuthEmail = "cookez.mail@gmail.com";
        public static string AuthPassword = "cookez";

        public ExternalLoginModel(
            SignInManager<BusinessObjects.Models.Account> signInManager,
            UserManager<BusinessObjects.Models.Account> userManager,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            RecipeOrganizerContext context) {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel {
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "Username")]
            [RegularExpression(@"^[a-z0-9]+$",
         ErrorMessage = "Username must contain only lowercase letter and digit.")]
            [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
            public string Username { get; set; }

            [Required]
            [Display(Name = "Profile image")]
            public IFormFile File { get; set; }
        }

        public IActionResult OnGetAsync() {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null) {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null) {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null) {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded) {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut) {
                return RedirectToPage("./Lockout");
            } else {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email)) {
                    Input = new InputModel {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null) {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            if (ModelState.IsValid) {
                var GgEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
                ViewData["GgEmail"] = GgEmail;
                var provider = info.LoginProvider;

                //var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };

                IFormFile file = Input.File;
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string imageUrl = await UploadFirebase(file.OpenReadStream(), uniqueFileName);
                Uri imageUrlUri = new Uri(imageUrl);
                string baseUrl = $"{imageUrlUri.GetLeftPart(UriPartial.Path)}?alt=media";
                
                var user = new BusinessObjects.Models.Account {
                    UserName = Input.Username,
                    Email = Input.Email,
                    ImgPath = baseUrl,
                    Status = true,
                    CreatedDate = DateTime.UtcNow
                };
                var up = new Favourite() {
                    Name = "Favourite",
                    Account = user,
                    isPrivate = true,
                };
                _context.Favourites.Add(up);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded) {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded) {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        await _userManager.AddToRoleAsync(user, "Cooker");
                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        string encodedCallbackUrl = HtmlEncoder.Default.Encode(callbackUrl);
                        string subject = "Confirm your email";
                        //string body = $"Please confirm your account by clicking <a href='{encodedCallbackUrl}'>here</a>.";

                        string body = $"<html><head> <style> body {{ font-family: Franklin Gothic Medium\t\r\n; line-height: 1.5; }} .container {{ max-width: 600px; margin: 0 auto; padding: 0 0 40px 0; }} h2 {{ font-size: 24px; margin-bottom: 20px; }} p {{ font-size: 16px; margin-bottom: 10px; }} a {{ color: #007bff; text-decoration: none; }} </style></head><body> <div class=\"container\" style=\"background-color: #F9F9F9;\"> <img src=\"https://media.istockphoto.com/id/1457889029/photo/group-of-food-with-high-content-of-dietary-fiber-arranged-side-by-side.jpg?b=1&s=612x612&w=0&k=20&c=BON5S0uDJeCe66N9klUEw5xKSGVnFhcL8stPLczQd_8=\" style=\"width: 100%;\"> <h2 style=\"margin-left: 25%; margin-top:3%; color: #ffcc21; font-size: 32px; font-family: Lucida Sans Typewriter;\">Welcome to Cookez</h2> <p style=\"margin-left: 16%; font-family: Lucida Sans Typewriter; color: black;\">You're almost ready to start enjoying Cookez</p> <p style=\"margin-left: 10%; font-family: Lucida Sans Typewriter; font-size: 14px; color:black;\">Simply click the button below to verify your email address.</p> <p style=\"margin-left:35%; margin-top: 4%;\"> <a href=\"{encodedCallbackUrl}\" style=\"display: inline-block; background-color: #F1C83E; color: #ffffff; text-decoration: none; padding: 10px 20px; border-radius: 4px;\">Verify your address</a> </p> </div></body></html>";

                        await _emailSender.SendEmailAsync(Input.Email, subject, body);

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount) {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                        return LocalRedirect(returnUrl);
                    }
                } else {
                    // Registration failed, remove the added favourite
                    _context.Favourites.Remove(up);
                    await _context.SaveChangesAsync();
                    foreach (var error in result.Errors) {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }


        public static async Task<string> UploadFirebase(Stream stream, string fileName) {
            string imageFromFirebaseStorage = "";

            using (Image image = Image.Load(stream)) {

                // Resize the image to a smaller size if needed
                int maxWidth = 1000; // Set your desired maximum width here
                int maxHeight = 1000; // Set your desired maximum height here
                if (image.Width > maxWidth || image.Height > maxHeight) {
                    image.Mutate(x => x.Resize(new ResizeOptions {
                        Size = new Size(maxWidth, maxHeight),
                        Mode = ResizeMode.Max
                    }));
                }

                using (MemoryStream webpStream = new ()) {

                    await image.SaveAsync(webpStream, new WebpEncoder());

                    webpStream.Position = 0;

                    FirebaseAuthProvider firebaseConfiguration = new(new FirebaseConfig(ApiKey));

                    FirebaseAuthLink authConfiguration = await firebaseConfiguration
                        .SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

                    CancellationTokenSource cancellationToken = new();

                    FirebaseStorageTask storageManager = new FirebaseStorage(
                        Bucket,
                        new FirebaseStorageOptions {
                            AuthTokenAsyncFactory = () => Task.FromResult(authConfiguration.FirebaseToken),
                            ThrowOnCancel = true
                        })
                        .Child("images")
                        .Child("avatar")
                        .Child(fileName)
                        .PutAsync(webpStream, cancellationToken.Token);

                    try {
                        imageFromFirebaseStorage = await storageManager;
                        firebaseConfiguration.Dispose();
                        return imageFromFirebaseStorage;
                    } catch (Exception ex) {
                        Console.WriteLine("Exception was thrown: {0}", ex);
                        return null;
                    }
                }
            }
        }


    }
}