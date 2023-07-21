using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartBreadcrumbs.Nodes;
using System;
using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Repositories;
using BusinessObjects.Models;

namespace CapstoneProject.Controllers {

    [DefaultBreadcrumb]
    public class HomeController : Controller {

        private readonly UserManager<Account> _userManager;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IRecipeCategoryRepository _recipeCategoryRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IFavouriteRepository _favouriteRepository;
        private readonly IIngredientCategoryRepository _ingredientCategoryRepository;
        private readonly IIngredientRepository _ingredientRepository;

        public HomeController(
            UserManager<Account> userManager,
            IRecipeRepository recipeRepository,
            IRecipeCategoryRepository recipeCategoryRepository,
            IAccountRepository accountRepository,
            IFavouriteRepository favouriteRepository,
            IIngredientCategoryRepository ingredientCategoryRepository,
            IIngredientRepository ingredientRepository) {
            _userManager = userManager;
            _recipeRepository = recipeRepository;
            _recipeCategoryRepository = recipeCategoryRepository;
            _accountRepository = accountRepository;
            _favouriteRepository = favouriteRepository;
            _ingredientCategoryRepository = ingredientCategoryRepository;
            _ingredientRepository = ingredientRepository;
        }

        public async Task<IActionResult> Index() {

            var recipes = _recipeRepository.GetRecipesHomeIndex();

            if (recipes != null) {
                var hotRecipe = _recipeRepository.GetHotRecipe();
                ViewData["HotRecipe"] = hotRecipe;
            }

            //Favourite list
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
                ViewBag.FavouriteList = _accountRepository.GetAccounts().FirstOrDefault(u => u.Id == currentUser.Id).Favourites.Select(f => new { f.Id, f.Name }).ToList();
            else
                ViewBag.FavouriteList = null;

            return View(recipes);
        }

        [Breadcrumb("Privacy", FromAction = "Index", FromController = typeof(HomeController))]
        public IActionResult Privacy() {
            return View();
        }

        [Breadcrumb("Recipe Detail", FromAction = "Index", FromController = typeof(HomeController))]
        public async Task<IActionResult> RecipeDetail(int? id) {
            var recipe = _recipeRepository.GetRecipeForHomeRecipeDetails(id);
            //Favourite list
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
                ViewBag.FavouriteList = _accountRepository.GetAccounts().FirstOrDefault(u => u.Id == currentUser.Id).Favourites.Select(f => new { f.Id, f.Name }).ToList();
            else
                ViewBag.FavouriteList = null;
            return View(recipe);
        }

        [Breadcrumb("View Ingredients", FromAction = "Index", FromController = typeof(HomeController))]
        public IActionResult ViewIngredient(int id, int pg = 1) {

            IEnumerable<Ingredient> obj = _ingredientRepository.GetStatusTrueIngredients()
                .Where(i => i.FkCategoryId == id).ToList();

            const int pageSize = 10; // Number of ingredients in 1 page

            if (pg < 1)
                pg = 1;

            int recsCount = obj.Count();

            var pager = new Pager(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var data = obj.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;

            return View(data);
        }

        [Breadcrumb("Profile", FromAction = "Index", FromController = typeof(HomeController))]
        [Authorize]
        public async Task<IActionResult> UserProfile(string? userId) {
            var user = _accountRepository.GetAccounts().FirstOrDefault(a => a.Id.Equals(userId));
            var currentUser = await _userManager.GetUserAsync(User);

            var recipes = _recipeRepository.GetRecipesUserProfile(userId);

            var popularRecipes = recipes
               .Take(6)
               .ToList();

            if(user == currentUser) {
                var collections = _favouriteRepository.GetFavouritesUserProfile(user);
                ViewData["collections"] = collections;
            } else {
                var collections = _favouriteRepository.GetFavouritesUserProfile(user)
                .Where(a => a.isPrivate == false); 
                ViewData["collections"] = collections;
            }
            ViewData["popularRecipes"] = popularRecipes;
            return View(user);
        }

        [Breadcrumb("My Recipes", FromAction = "Index", FromController = typeof(HomeController))]
        [Authorize]
        public async Task<IActionResult> MyRecipes() {
            var currentUser = await _userManager.GetUserAsync(User);

            var recipes = _recipeRepository.GetRecipesMyRecipes(currentUser);
            return View(recipes);
        }

        public async Task<IActionResult> Favorite(int? id, int? favouriteId, string returnUrl, string parameters) {
            var entity = _recipeRepository.GetRecipeById(id);
            if (entity != null) {
                var currentUser = await _userManager.GetUserAsync(User);
                var userFavouriteId = _accountRepository.GetAccounts().FirstOrDefault(u => u.Id == currentUser.Id).Favourites.ToArray();
                favouriteId = userFavouriteId[0].Id;
                //Get user from dbContext which include favorites
                var favourite = _favouriteRepository.GetFavouriteById(favouriteId);

                if (!favourite.Recipes.Contains(entity)) {
                    _favouriteRepository.InsertRecipeToFavourite(favourite, entity);
                    TempData["success"] = "Add to favourites successfully";
                } else {
                    _favouriteRepository.DeleteRecipeFromFavourite(favourite, entity);
                    TempData["success"] = "Removed from favourites successfully";
                }
            }

            return LocalRedirect(returnUrl + parameters);
        }

        public IActionResult IngredientDetails(int? id, int? ingredientId) {
            var childNode1 = new MvcBreadcrumbNode("ViewIngredient", "Home", "View Ingredients", false) {
                RouteValues = new { id } //this comes in as a param into the action
            };
            var childNode2 = new MvcBreadcrumbNode("IngredientDetails", "Home", "Ingredient Details") {
                OverwriteTitleOnExactMatch = true,
                Parent = childNode1
            };

            if (id == null || _ingredientRepository.GetIngredients() == null) {
                return NotFound();
            }

            var ingredient = _ingredientRepository.GetIngredientById(ingredientId);
            if (ingredient == null) {
                return NotFound();
            }

            ViewData["BreadcrumbNode"] = childNode2;
            return View(ingredient);
        }

        public IActionResult Contact() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}