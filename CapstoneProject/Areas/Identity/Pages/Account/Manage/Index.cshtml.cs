using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats;
using NuGet.Protocol;
using SixLabors.ImageSharp.Formats.Webp;

namespace CapstoneProject.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        public static string ApiKey = "AIzaSyDIXdDdvo8NguMgxLvn4DWMNS-vXkUxoag";
        public static string Bucket = "cookez-cloud.appspot.com";
        public static string AuthEmail = "cookez.mail@gmail.com";
        public static string AuthPassword = "cookez";

        private readonly UserManager<BusinessObjects.Models.Account> _userManager;
        private readonly SignInManager<BusinessObjects.Models.Account> _signInManager;

        public IndexModel(
            UserManager<BusinessObjects.Models.Account> userManager,
            SignInManager<BusinessObjects.Models.Account> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Profile Image")]
            public IFormFile? File { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync((BusinessObjects.Models.Account)user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync((BusinessObjects.Models.Account)user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            if (Input.File != null) {
                IFormFile file = Input.File;
                var currImgSrc = _userManager.GetUserAsync(User).Result.ImgPath;
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string imageUrl = await RemoveOldAndUploadFirebase(file.OpenReadStream(), uniqueFileName, currImgSrc);
                Uri imageUrlUri = new Uri(imageUrl);
                string baseUrl = $"{imageUrlUri.GetLeftPart(UriPartial.Path)}?alt=media";
                user.ImgPath = baseUrl;
            }
            
            await _userManager.UpdateAsync(user);
            TempData["success"] = "Profile information updated successfully";
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage();
        }

        public static async Task<string> RemoveOldAndUploadFirebase(Stream stream, string fileName, string deletedFile)
        {
            if (!string.IsNullOrEmpty(deletedFile)) {
                string exactFileName = GetImageNameFromUrl(deletedFile);
                FirebaseAuthProvider firebaseConfiguration = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                FirebaseAuthLink authConfiguration = await firebaseConfiguration
                    .SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

                FirebaseStorage storage = new FirebaseStorage(
                    Bucket,
                    new FirebaseStorageOptions {
                        AuthTokenAsyncFactory = () => Task.FromResult(authConfiguration.FirebaseToken),
                        ThrowOnCancel = true
                    });

                await storage
                    .Child("images")
                    .Child("avatar")
                    .Child(exactFileName) // Use the fileName directly as the child reference
                    .DeleteAsync();

                firebaseConfiguration.Dispose();
            }

            string imageFromFirebaseStorage = "";

            using (Image image = Image.Load(stream))
            {

                // Resize the image to a smaller size if needed
                int maxWidth = 1000; // Set your desired maximum width here
                int maxHeight = 1000; // Set your desired maximum height here
                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
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
                        new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(authConfiguration.FirebaseToken),
                            ThrowOnCancel = true
                        })
                        .Child("images")
                        .Child("avatar")
                        .Child(fileName)
                        .PutAsync(webpStream, cancellationToken.Token);
                    try
                    {
                        imageFromFirebaseStorage = await storageManager;
                        firebaseConfiguration.Dispose();
                        return imageFromFirebaseStorage;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception was thrown: {0}", ex);
                        return null;
                    }
                }
            }
        }

        public static string GetImageNameFromUrl(string url) {
            int lastSeparatorIndex = url.LastIndexOf("%2F"); // Get the index of the last "%2F"
            int startIndex = lastSeparatorIndex + 3; // Start index of the desired string
            int endIndex = url.IndexOf("?alt", startIndex); // End index of the desired string
            string extractedString = url.Substring(startIndex, endIndex - startIndex); // Extract the string between the last "%2F" and before "?alt"
            string decodedString = System.Web.HttpUtility.UrlDecode(extractedString); // Decode the extracted string
            return decodedString;
        }
    }
}
