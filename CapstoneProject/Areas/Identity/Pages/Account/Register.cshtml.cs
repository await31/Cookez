// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using CapstoneProject.Services;
using CapstoneProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Firebase.Auth;
using Firebase.Storage;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;

namespace CapstoneProject.Areas.Identity.Pages.Account {
    public class RegisterModel : PageModel {
        private readonly SignInManager<BusinessObjects.Models.Account> _signInManager;
        private readonly UserManager<BusinessObjects.Models.Account> _userManager;
        private readonly IUserStore<BusinessObjects.Models.Account> _userStore;
        private readonly IUserEmailStore<BusinessObjects.Models.Account> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly BusinessObjects.Models.RecipeOrganizerContext _context;

        public static string ApiKey = "AIzaSyDIXdDdvo8NguMgxLvn4DWMNS-vXkUxoag";
        public static string Bucket = "cookez-cloud.appspot.com";
        public static string AuthEmail = "cookez.mail@gmail.com";
        public static string AuthPassword = "cookez";

        public RegisterModel(
            UserManager<BusinessObjects.Models.Account> userManager,
            IUserStore<BusinessObjects.Models.Account> userStore,
            SignInManager<BusinessObjects.Models.Account> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            BusinessObjects.Models.RecipeOrganizerContext context) {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<BusinessObjects.Models.Account>)GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel {

            [Required]
            [Display(Name = "Username")]
            [RegularExpression(@"^[a-z0-9]+$",
         ErrorMessage = "Username must contain only lowercase letter and digit.")]
            [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
            public string Username { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            public string? Role { get; set; }


            [Required]
            [Display(Name = "Profile image")]
            public IFormFile File { get; set; }

            /*[Required]
            [RegularExpression("True", ErrorMessage = "You must agree to our terms of service to continue register")]
             */
            [ValidateNever]
            public bool AgreeTerms { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null) {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            Input = new InputModel() {
                RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem {
                    Text = i,
                    Value = i
                })
            };
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid) {
                MailAddress address = new MailAddress(Input.Email);
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
                var up = new BusinessObjects.Models.Favourite() {
                    Name = "Favourite",
                    Account = user,
                    isPrivate= true,
                };
                _context.Favourites.Add(up);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded) {
                    await _userManager.AddToRoleAsync(user, "Cooker");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    string encodedCallbackUrl = HtmlEncoder.Default.Encode(callbackUrl);
                    string subject = "Confirm your email";
                    //string body = $"Please confirm your account by clicking <a href='{encodedCallbackUrl}'>here</a>.";

                    string body = $"<html><head> <style> body {{ font-family: Franklin Gothic Medium\t\r\n; line-height: 1.5; }} .container {{ max-width: 600px; margin: 0 auto; padding: 0 0 40px 0; }} h2 {{ font-size: 24px; margin-bottom: 20px; }} p {{ font-size: 16px; margin-bottom: 10px; }} a {{ color: #007bff; text-decoration: none; }} </style></head><body> <div class=\"container\" style=\"background-color: #F9F9F9;\"> <img src=\"https://media.istockphoto.com/id/1457889029/photo/group-of-food-with-high-content-of-dietary-fiber-arranged-side-by-side.jpg?b=1&s=612x612&w=0&k=20&c=BON5S0uDJeCe66N9klUEw5xKSGVnFhcL8stPLczQd_8=\" style=\"width: 100%;\"> <h2 style=\"margin-left: 25%; margin-top:3%; color: #ffcc21; font-size: 32px; font-family: Lucida Sans Typewriter;\">Welcome to Cookez</h2> <p style=\"margin-left: 16%; font-family: Lucida Sans Typewriter; color: black;\">You're almost ready to start enjoying Cookez</p> <p style=\"margin-left: 10%; font-family: Lucida Sans Typewriter; font-size: 14px; color:black;\">Simply click the button below to verify your email address.</p> <p style=\"margin-left:35%; margin-top: 4%;\"> <a href=\"{encodedCallbackUrl}\" style=\"display: inline-block; background-color: #F1C83E; color: #ffffff; text-decoration: none; padding: 10px 20px; border-radius: 4px;\">Verify your address</a> </p> </div></body></html>";


                    await _emailSender.SendEmailAsync(Input.Email, subject, body);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount) {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    } else {
                        await _signInManager.SignInAsync(user, isPersistent: false);
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

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private BusinessObjects.Models.Account CreateUser() {
            try {
                return Activator.CreateInstance<BusinessObjects.Models.Account>();
            } catch {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(BusinessObjects.Models.Account)}'. " +
                    $"Ensure that '{nameof(BusinessObjects.Models.Account)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<BusinessObjects.Models.Account> GetEmailStore() {
            if (!_userManager.SupportsUserEmail) {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<BusinessObjects.Models.Account>)_userStore;
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

                using (MemoryStream webpStream = new()) {
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