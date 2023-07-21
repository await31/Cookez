using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Firebase.Auth;
using Firebase.Storage;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using Microsoft.AspNetCore.WebUtilities;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using MailKit;
using System.Text.RegularExpressions;
using SmartBreadcrumbs.Attributes;
using BusinessObjects.Models;
using Repositories;

namespace CapstoneProject.Controllers {

    public class IngredientsController : Controller {

        public static string ApiKey = "AIzaSyDIXdDdvo8NguMgxLvn4DWMNS-vXkUxoag";
        public static string Bucket = "cookez-cloud.appspot.com";
        public static string AuthEmail = "cookez.mail@gmail.com";
        public static string AuthPassword = "cookez";

        private readonly IIngredientRepository _ingredientRepository;
        private readonly IIngredientCategoryRepository _ingredientCategoryRepository;

        public IngredientsController(IIngredientRepository ingredientRepository, IIngredientCategoryRepository ingredientCategoryRepository) {
            _ingredientRepository = ingredientRepository;
            _ingredientCategoryRepository = ingredientCategoryRepository;
        }
        [HttpPost]
        public JsonResult AutoComplete(string term)
        {
            var list = _ingredientRepository.GetIngredients();
            var result = (list.Where(i => i.Status == true).Where(t => t.Name.ToLower().Contains(term.ToLower()))
                 .Select(t => new { t.Name }))
                 .ToList();
            return Json(result);
        }

        [Authorize(Roles = "Admin")]
        [Breadcrumb("Ingredients Management")]
        public IActionResult Index() {
            var objIngredient = _ingredientRepository.GetIngredients();
            return View(objIngredient);
        }

        // GET
        [Breadcrumb("Create", FromAction = "Index", FromController = typeof(IngredientsController))]
        [Authorize(Roles = "Admin")]
        public IActionResult Create() {
            ViewData["FkCategoryId"] = new SelectList(_ingredientCategoryRepository.GetIngredientCategories(), "Id", "Name");
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ingredient model) {
            if (ModelState.IsValid) {
                if (model.file != null && model.file.Length > 0) {
                    IFormFile file = model.file;
                    // Generate a unique file name
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    // Upload the file to Firebase Storage
                    string imageUrl = await UploadFirebase(file.OpenReadStream(), uniqueFileName);
                    Uri imageUrlUri = new Uri(imageUrl);
                    string baseUrl = $"{imageUrlUri.GetLeftPart(UriPartial.Path)}?alt=media";
                    model.ImgPath = baseUrl;
                    model.Status = true;
                    _ingredientRepository.InsertIngredient(model);
                    TempData["success"] = "Ingredient created successfully";
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAjax(Ingredient model) {
            if (ModelState.IsValid) {
                if (model.file != null && model.file.Length > 0) {
                    IFormFile file = model.file;
                    // Generate a unique file name
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    // Upload the file to Firebase Storage
                    string imageUrl = await UploadFirebase(file.OpenReadStream(), uniqueFileName);
                    Uri imageUrlUri = new Uri(imageUrl);
                    string baseUrl = $"{imageUrlUri.GetLeftPart(UriPartial.Path)}?alt=media";
                    model.ImgPath = baseUrl;
                    model.Status = false;
                    _ingredientRepository.InsertIngredient(model);
                    TempData["success"] = "Ingredient created successfully";
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }

        // GET: Ingredient/Detail/id
        [Authorize(Roles = "Admin")]
        public IActionResult Detail(int? id) {
            if (id == null) {
                return NotFound();
            }

            var ingredient = _ingredientRepository.GetIngredientById(id);

            if (ingredient == null) {
                return NotFound();
            }

            return View(ingredient);
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
                        .Child("ingredients")
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


        //GET
        [Breadcrumb("Edit", FromAction = "Index", FromController = typeof(IngredientsController))]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var ingredient = _ingredientRepository.GetIngredientById(id);

            if (ingredient == null) {
                return NotFound();
            }

            ViewData["FkCategoryId"] = new SelectList(_ingredientCategoryRepository.GetIngredientCategories(), "Id", "Name");
            return View(ingredient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id, [Bind("Id, Name, ImgPath, Description, Status, FkCategoryId")] Ingredient ingredient) {
            if (id != ingredient.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _ingredientRepository.UpdateIngredient(ingredient);
                    TempData["success"] = "Ingredient updated successfully";
                } catch (Exception) {
                    return NotFound();
                }
                return RedirectToAction("Index");
            }

            ViewData["FkCategoryId"] = new SelectList(_ingredientCategoryRepository.GetIngredientCategories(), "Id", "Name");
            return View(ingredient);
        }

        private bool IngredientExists(int id) {
            return (_ingredientRepository.GetIngredients()?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // GET
        [Breadcrumb("Delete", FromAction = "Index", FromController = typeof(IngredientsController))]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var ingredient = _ingredientRepository.GetIngredientById(id);

            if (ingredient == null) {
                return NotFound();
            }

            return View(ingredient);
        }

        /*
         [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePOST(int? id) {
            var obj = _context.Ingredients.Find(id);
            if (obj == null) {
                return NotFound();
            }

            await DeleteFromFirebaseStorage(obj.ImgPath);
            var ingredientListInRecipe = _context.RecipeIngredient.Where(ri => ri.IngredientId == obj.Id);
            _context.Ingredients.Remove(obj);
            _context.RemoveRange(ingredientListInRecipe);
            _context.SaveChanges();
            TempData["success"] = "Ingredient deleted successfully";
            return RedirectToAction("Index");
        }
         */

        // POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var ingredient = _ingredientRepository.GetIngredientById(id);
            await DeleteFromFirebaseStorage(ingredient.ImgPath);
            _ingredientRepository.DeleteIngredient(ingredient);
            TempData["success"] = "Ingredient deleted successfully";
            return RedirectToAction("Index");
        }


        // POST: Recipes/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Approve(int id) {
            var ingredient = _ingredientRepository.GetIngredientById(id);
            if (ingredient == null) {
                return NotFound();
            }
            _ingredientRepository.Approve(ingredient);
            return RedirectToAction("Index", "Dashboard");
        }
        [Authorize]
        // POST: Recipes/Deny
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deny(int id) {
            var ingredient = _ingredientRepository.GetIngredientById(id);
            if (ingredient == null) {
                return NotFound();
            }
            await DeleteFromFirebaseStorage(ingredient.ImgPath);
            _ingredientRepository.Deny(ingredient);
            return RedirectToAction("Index", "Dashboard");
        }

        private async Task DeleteFromFirebaseStorage(string fileName) {
            if (!string.IsNullOrEmpty(fileName)) {
                string exactFileName = GetImageNameFromUrl(fileName);
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
                    .Child("ingredients")
                    .Child(exactFileName) // Use the fileName directly as the child reference
                    .DeleteAsync();

                firebaseConfiguration.Dispose();
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


