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
using NuGet.Packaging;
using SmartBreadcrumbs.Attributes;
using Microsoft.AspNetCore.Identity;
using BusinessObjects.Models;
using Repositories;

namespace CapstoneProject.Controllers {


    [Authorize(Roles = "Admin")]
    public class RecipesController : Controller {
        private readonly UserManager<Account> _userManager;

        private readonly IRecipeRepository _recipeRepository;
        private readonly IRecipeCategoryRepository _recipeCategoryRepository;

        public static string ApiKey = "AIzaSyDIXdDdvo8NguMgxLvn4DWMNS-vXkUxoag";
        public static string Bucket = "cookez-cloud.appspot.com";
        public static string AuthEmail = "cookez.mail@gmail.com";
        public static string AuthPassword = "cookez";

        public RecipesController(UserManager<Account> userManager, IRecipeRepository recipeRepository, IRecipeCategoryRepository recipeCategoryRepository) {
            _userManager = userManager;
            _recipeRepository = recipeRepository;
            _recipeCategoryRepository = recipeCategoryRepository;
        }

        // GET/POST: Recipes
        [Breadcrumb("Recipes Management")]

        public IActionResult Index(int pg = 1) {
            var recipes = _recipeRepository.GetRecipes();

            const int pageSize = 10; 

            if (pg < 1)
                pg = 1;

            int recsCount = recipes.Count();

            var pager = new Pager(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var data = recipes.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;

            return View(data);
        }

        // GET: Recipes/Details/5
        public IActionResult Details(int? id) {

            if (id == null || _recipeRepository.GetRecipes() == null) {
                return NotFound();
            }

            var recipe = _recipeRepository.GetRecipeById(id);
            if (recipe == null) {
                return NotFound();
            }

            return View(recipe);
        }

        [Authorize]
        // GET: Recipes/Delete/5
        public IActionResult Delete(int? id) {
            if (id == null || _recipeRepository.GetRecipes() == null) {
                return NotFound();
            }

            var recipe = _recipeRepository.GetRecipeById(id);
            if (recipe == null) {
                return NotFound();
            }

            return View(recipe);
        }
        [Authorize]
        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            if (_recipeRepository.GetRecipes() == null) {
                return Problem("Entity set 'RecipeOrganizerContext.Recipes'  is null.");
            }

            var recipe = _recipeRepository.GetRecipeById(id);
            if (recipe != null) {
                if (recipe.ImgPath != null) {
                    await DeleteFromFirebaseStorage(recipe.ImgPath);
                }
                _recipeRepository.DeleteRecipe(recipe);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RecipeExists(int id) {
            return (_recipeRepository.GetRecipes()?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [Authorize]
        // POST: Recipes/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id) {
            var recipe = _recipeRepository.GetRecipeById(id);
            if (recipe == null) {
                return NotFound();
            }
            _recipeRepository.Approve(recipe);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        // POST: Recipes/Deny
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deny(int id) {
            var recipe = _recipeRepository.GetRecipeById(id);
            if (recipe == null) {
                return NotFound();
            }
            await DeleteFromFirebaseStorage(recipe.ImgPath);
            _recipeRepository.Deny(recipe);
            return RedirectToAction(nameof(Index));
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
                    .Child("recipes")
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