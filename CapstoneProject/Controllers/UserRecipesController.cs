using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Collections;
using Microsoft.IdentityModel.Tokens;
using SmartBreadcrumbs.Attributes;
using NuGet.Packaging;
using Firebase.Auth;
using Firebase.Storage;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats;
using System.Xml.Linq;
using SixLabors.ImageSharp.Formats.Webp;
using Microsoft.AspNetCore;
using System.Globalization;
using Repositories;
using BusinessObjects.Models;
using Org.BouncyCastle.Utilities;

namespace CapstoneProject.Controllers {

    public class UserRecipesController : Controller {

        private readonly UserManager<Account> _userManager;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IRecipeCategoryRepository _recipeCategoryRepository;
        private readonly IFavouriteRepository _favouriteRepository;
        private readonly IRecipeFeedbackRepository _feedbackRepository;
        private readonly IIngredientCategoryRepository _ingredientCategoryRepository;
        private readonly IIngredientRepository _ingredientRepository;

        public static string ApiKey = "AIzaSyDIXdDdvo8NguMgxLvn4DWMNS-vXkUxoag";
        public static string Bucket = "cookez-cloud.appspot.com";
        public static string AuthEmail = "cookez.mail@gmail.com";
        public static string AuthPassword = "cookez";
        public UserRecipesController(
            UserManager<Account> userManager,
            IRecipeRepository recipeRepository,
            IRecipeCategoryRepository recipeCategoryRepository,
            IAccountRepository accountRepository,
            IFavouriteRepository favouriteRepository,
            IRecipeFeedbackRepository feedbackRepository,
            IIngredientCategoryRepository ingredientCategoryRepository,
            IIngredientRepository ingredientRepository) {
            _userManager = userManager;
            _recipeRepository = recipeRepository;
            _accountRepository = accountRepository;
            _recipeCategoryRepository = recipeCategoryRepository;
            _favouriteRepository = favouriteRepository;
            _feedbackRepository = feedbackRepository;
            _ingredientCategoryRepository = ingredientCategoryRepository;
            _ingredientRepository = ingredientRepository;
        }

        [HttpPost]
        public JsonResult AutoComplete(string term) {
            var result = (_recipeRepository.GetRecipes().Where(t => t.Name.ToLower().Contains(term.ToLower()))
                 .Select(t => new { t.Name }))
                 .ToList();
            return Json(result);
        }

        private List<SelectListItem> GetDifficultyList() {
            return new List<SelectListItem>
                    {
                    new SelectListItem { Text = "Easy", Value = "1"},
                    new SelectListItem { Text = "Medium", Value = "2"},
                    new SelectListItem { Text = "Hard", Value = "3"},
                    };
        }
        private List<SelectListItem> GetPrepTimeList() {
            var list = new List<SelectListItem>();
            for (int i = 5; i < 30; i += 5) {
                list.Add(new SelectListItem { Text = i + " minutes", Value = i.ToString() });
            }
            for (int i = 30; i <= 360; i += 30) {
                list.Add(new SelectListItem { Text = i + " minutes", Value = i.ToString() });
            }
            for (int i = 420; i <= 720; i += 60) {
                list.Add(new SelectListItem { Text = i / 60 + " hours", Value = i.ToString() });
            }
            return list;
        }
        private List<SelectListItem> GetUnitsofMeasureList() {
            return new List<SelectListItem>
                    {
                    new SelectListItem { Text = "milliliters", Value = "milliliters"},
                    new SelectListItem { Text = "grams", Value = "grams"},
                    };
        }
        // GET: Recipes
        [Breadcrumb("Recipes")]
        public async Task<IActionResult> Index(int pg = 1) {
            const int pageSize = 6; // Number of recipes in 1 page
            if (pg < 1)
                pg = 1;
            var recipes = _recipeRepository.GetRecipesIndex();
            if (recipes != null) {
                string? searchString = Request.Query["SearchString"];
                string? prepTime = Request.Query["PrepTime"];
                string? recipeCategory = Request.Query["RecipeCategory"];
                string? difficulty = Request.Query["Difficulty"];
                string? sortBy = Request.Query["SortBy"];
                string? includeList = Request.Query["IncludeList"];
                string? excludeList = Request.Query["ExcludeList"];

                //Add all recipecategories
                if (String.IsNullOrEmpty(prepTime))
                    prepTime = "All";
                if (String.IsNullOrEmpty(difficulty))
                    difficulty = "All";
                if (String.IsNullOrEmpty(recipeCategory))
                    recipeCategory = "All";
                if (String.IsNullOrEmpty(sortBy))
                    sortBy = "SortPopular";
                ViewBag.FkRecipeCategoryId = new SelectList(_recipeCategoryRepository.GetRecipeCategories(), "Id", "Name", recipeCategory);
                ViewBag.PrepTime = new SelectList(GetPrepTimeList(), "Value", "Text", prepTime);
                ViewBag.Difficulty = new SelectList(GetDifficultyList(), "Value", "Text", difficulty);
                ViewBag.SortBy = new SelectList(
                new List<SelectListItem>
                {
                new SelectListItem { Text = "Popularity", Value = "SortPopular"},
                new SelectListItem { Text = "Name", Value = "SortName"},
                new SelectListItem { Text = "Newest", Value = "SortNewest"},
                new SelectListItem { Text = "Oldest", Value = "SortOldest"},
                new SelectListItem { Text = "Preparation time", Value = "SortPrepTime"},
                }
                , "Value", "Text", sortBy);

                //View Data
                ViewData["FilterSearch"] = searchString;
                ViewData["FilterPrepTime"] = prepTime;
                ViewData["FilterDifficulty"] = difficulty;
                ViewData["FilterIncludeList"] = includeList;
                ViewData["FilterExcludeList"] = excludeList;
                //Include Ingredients
                if (!prepTime.Equals("All")) {
                    recipes = recipes.Where(b => b.PrepTime <= int.Parse(prepTime));
                }
                if (!difficulty.Equals("All")) {
                    recipes = recipes.Where(b => b.Difficult == int.Parse(difficulty));
                }
                if (!recipeCategory.Equals("All")) {
                    recipes = recipes.Where(b => b.FkRecipeCategoryId == int.Parse(recipeCategory));
                }
                if (!String.IsNullOrEmpty(searchString)) {
                    recipes = recipes.Where(b => b.Name.ToLower().Contains(searchString.ToLower()));
                }
                //Sort
                switch (sortBy) {
                    case "SortOldest":
                        recipes = recipes.OrderBy(b => b.CreatedDate);
                        break;
                    case "SortNewest":
                        recipes = recipes.OrderByDescending(b => b.CreatedDate);
                        break;
                    case "SortPrepTime":
                        recipes = recipes.OrderBy(b => b.PrepTime);
                        break;
                    case "SortName":
                        recipes = recipes.OrderBy(b => b.Name);
                        break;
                    default: // Sort by most popular
                        recipes = recipes.OrderByDescending(b => b.ViewCount);
                        break;
                }
                if (!String.IsNullOrEmpty(includeList)) {
                    var ingredientsList = includeList.Split(',');
                    ViewBag.IncludeList = ingredientsList;
                    foreach (var item in ingredientsList) {
                        recipes = recipes.Where(b => b.Ingredients.Any(i => i.Name.Equals(item)));
                    }
                } else
                    ViewBag.IncludeList = new List<String>();
                //Exclude
                if (!String.IsNullOrEmpty(excludeList)) {
                    var ingredientsList = excludeList.Split(',');
                    ViewBag.ExcludeList = ingredientsList;
                    foreach (var item in ingredientsList) {
                        recipes = recipes.Where(b => b.Ingredients.All(i => !i.Name.Equals(item)));
                    }
                } else
                    ViewBag.ExcludeList = new List<String>();

                //Favourite list
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                    ViewBag.FavouriteList = _accountRepository.GetAccounts().FirstOrDefault(u => u.Id == currentUser.Id).Favourites.Select(f => new { f.Id, f.Name }).ToList();
                else
                    ViewBag.FavouriteList = null;

                int recsCount = recipes.Count();

                var pager = new Pager(recsCount, pg, pageSize, includeList, excludeList, recipeCategory, prepTime, difficulty, sortBy);

                int recSkip = (pg - 1) * pageSize;

                var data = recipes.Skip(recSkip).Take(pager.PageSize).ToList();

                ViewData["count"] = recsCount;

                this.ViewBag.Pager = pager;
                return View(data);
            }
            return View();
        }
        [HttpPost]
        public IActionResult Filter(string searchString, string recipeCategory, string prepTime, string difficulty,
            string sortBy, string includeIngredients, string includeIngredientsList, string excludeIngredients, string excludeIngredientsList, int pg = 1) {
            string? includeList = includeIngredientsList;
            string? excludeList = excludeIngredientsList;
            if (!String.IsNullOrEmpty(includeIngredients)) {
                //Remove from exclude list if it has the include ingre inputed
                if (!String.IsNullOrEmpty(excludeList)) {
                    if (excludeList.IndexOf(includeIngredients, StringComparison.OrdinalIgnoreCase) != -1)
                        excludeList = RemoveFromStringWithComma(includeIngredients, excludeList);
                }
                if (String.IsNullOrEmpty(includeList)) {
                    includeList = includeIngredients.Trim();
                } else
                    includeList += "," + includeIngredients.Trim();
            }
            if (!String.IsNullOrEmpty(excludeIngredients)) {
                //Remove from include list if it has the exclude ingre inputed
                if (!String.IsNullOrEmpty(includeList)) {
                    if (includeList.IndexOf(excludeIngredients, StringComparison.OrdinalIgnoreCase) != -1)
                        includeList = RemoveFromStringWithComma(excludeIngredients, includeList);
                }
                if (String.IsNullOrEmpty(excludeList)) {
                    excludeList = excludeIngredients.Trim();
                } else
                    excludeList += "," + excludeIngredients.Trim();
            }
            var parameter = new RouteValueDictionary
            {
                { "pg",  pg},
                { "SearchString", searchString },
                { "RecipeCategory", recipeCategory },
                { "PrepTime", prepTime },
                { "Difficulty", difficulty },
                { "SortBy", sortBy },
                { "IncludeList", includeList },
                { "ExcludeList", excludeList }
            };
            return RedirectToAction("Index", parameter);
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

        private string RemoveFromStringWithComma(string remove, string full) {
            full = full.Replace("," + remove, "");
            full = full.Replace(remove + ",", "");
            full = full.Replace(remove, "");
            return full;
        }
        private List<RecipeFeedback> GetRecipeFeedbacks(int recipeId) => (List<RecipeFeedback>)_feedbackRepository.GetRecipeFeedbacks(recipeId);
        private List<RecipeFeedback> GetRecipeFeedbackPage(List<RecipeFeedback> feedbacks, int pg) {
            const int pageSize = 5; // Number of comments in 1 page
            if (pg < 1)
                pg = 1;

            int recsCount = feedbacks.Count();

            var pager = new Pager(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            this.ViewBag.Pager = pager;

            var data = feedbacks.Skip(recSkip).Take(pager.PageSize).ToList();
            return data;
        }
        [Breadcrumb("Details")]
        // GET: Recipes/Details/5
        public IActionResult Details(int id, int pg = 1) {
            var recipe = _recipeRepository.GetRecipeForDetails(id);
            var feedbacks = GetRecipeFeedbacks(id);
            var data = GetRecipeFeedbackPage(feedbacks, pg);
            ViewData["feedbacks"] = data;
            ViewData["RecipeId"] = id;

            if (recipe != null) {
                recipe.ViewCount++;
            }

            var suggestRecipes = _recipeRepository.GetSuggestRecipes(id);

            ViewData["footerRecipes"] = suggestRecipes;

            //Favourite list
            var currentUser = _userManager.GetUserId(User);
            if (currentUser != null)
                ViewBag.FavouriteList = _accountRepository.GetAccounts().FirstOrDefault(u => u.Id == currentUser).Favourites.Select(f => new { f.Id, f.Name }).ToList();
            else
                ViewBag.FavouriteList = null;
            return View(recipe);
        }

        [HttpPost]
        public async Task<IActionResult> GetFeedbackAsync(string feedbackText, int recipeId, int pg = 1) {
            var currentUser = await _userManager.GetUserAsync(User);
            if (feedbackText != null) {
                if (CompareLimitedWords(feedbackText) == true) {
                    TempData["error"] = "Your feedback was ignored because it contains an invalid word.";
                } else {
                    var feedback = new RecipeFeedback {
                        RecipeId = recipeId,
                        UserId = currentUser.Id,
                        Description = feedbackText,
                        Rating = 0,
                        CreatedDate = DateTime.Now
                    };
                    _feedbackRepository.InsertRecipeFeedback(feedback);
                }
            }
            var feedbacks = GetRecipeFeedbacks(recipeId);
            var data = GetRecipeFeedbackPage(feedbacks, pg);
            ViewData["RecipeId"] = recipeId;
            return PartialView("_Feedback", data);
        }

        [HttpPost]
        public IActionResult DeleteFeedbackAsync(int recipeId, int id, int pg = 1) {
            var feedback = _feedbackRepository.GetRecipeFeedbackById(id);
            if (feedback != null) {
                _feedbackRepository.DeleteRecipeFeedback(feedback);
            }
            var feedbacks = GetRecipeFeedbacks(recipeId);
            var data = GetRecipeFeedbackPage(feedbacks, pg);
            ViewData["RecipeId"] = recipeId;
            return PartialView("_Feedback", data);
        }

        public bool CompareLimitedWords(string text) {
            List<string> words = new List<string>() { "stupid", "idiot", "disgusting", "shit", "jesus", "ass", "damn", "fuck", "asshole", "bastard", "bullshit", "cunt", "dick", "dyke", "hell", "holy shit", "holyshit", "motherfucker", "mother fucker", "nigga", "nigra", "n1gga", "pussy", "slut", "turd", "wanker", "wtf" };
            foreach (string word in words) {
                if (text.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0) {
                    return true;
                }
            }

            return false;
        }

        // GET: Recipes/Create
        [Breadcrumb("Create recipe")]
        [Authorize]
        public IActionResult Create() {
            ViewData["FkRecipeId"] = new SelectList(_recipeRepository.GetRecipes(), "Id", "Id");
            ViewData["FkRecipeCategoryId"] = new SelectList(_recipeCategoryRepository.GetRecipeCategories(), "Id", "Name");
            IEnumerable<IngredientCategory> ingCategoryList = _ingredientCategoryRepository.GetIngredientCategories();

            // lay danh sach ingredient tu database
            var ingredients = _ingredientRepository.GetStatusTrueIngredients();
            ViewData["IngCategories"] = ingCategoryList;
            ViewData["Ingredients"] = new SelectList(ingredients, "Id", "Name"); // truyen qua view create

            return View();
        }


        // POST: Recipes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Recipe recipe, string[] IngredientNames, double[] Quantities, string[] UnitOfMeasures) {
            if (ModelState.IsValid) {
                if (recipe.file != null && recipe.file.Length > 0) {
                    IFormFile file = recipe.file;
                    // Generate a unique file name
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null) {
                        // Upload the file to Firebase Storage
                        string imageUrl = await UploadFirebase(file.OpenReadStream(), uniqueFileName);
                        Uri imageUrlUri = new(imageUrl);
                        string baseUrl = $"{imageUrlUri.GetLeftPart(UriPartial.Path)}?alt=media";
                        recipe.ImgPath = baseUrl;
                        recipe.Status = false;
                        recipe.ViewCount = 0;
                        recipe.CreatedDate = DateTime.Now;
                        recipe.FkUserId = await _userManager.GetUserIdAsync(currentUser);
                        if (IngredientNames != null) {
                            var allIngredients = _ingredientRepository.GetStatusTrueIngredients();

                            var ingredients = allIngredients
                                              .Where(i => IngredientNames.Any(input => i.Name.Equals(input)))
                                              .ToList();

                            var IngredientIds = ingredients.Select(i => i.Id).ToArray();
                            List<RecipeIngredient> recipeIngredients = new();
                            Nutrition recipeNutrition = new Nutrition {
                                Calories = 0,
                                Fat = 0,
                                Protein = 0,
                                Fibre = 0,
                                Carbohydrate = 0,
                                Cholesterol = 0
                            };

                            for (int i = 0; i < IngredientNames.Length; i++) {
                                var list = _ingredientRepository.GetStatusTrueIngredients();
                                bool exists = list.Any(ingredient =>
                                                    ingredient.Name.Equals(IngredientNames[i], StringComparison.OrdinalIgnoreCase));
                                                                                                
                                if (exists) {
                                    getIngredientIdFromName(IngredientNames[i], out int id);
                                    recipeIngredients.Add(new RecipeIngredient {
                                        IngredientId = id,
                                        RecipeId = recipe.Id,
                                        Quantity = Quantities[i],
                                        UnitOfMeasure = UnitOfMeasures[i]
                                    });

                                    var ingredientNutritionCalories = _ingredientRepository.GetIngredientById(id).IngredientNutrition.Calories;
                                    var ingredientNutritionFat = _ingredientRepository.GetIngredientById(id).IngredientNutrition.Fat;
                                    var ingredientNutritionProtein = _ingredientRepository.GetIngredientById(id).IngredientNutrition.Protein;
                                    var ingredientNutritionFibre = _ingredientRepository.GetIngredientById(id).IngredientNutrition.Fibre;
                                    var ingredientNutritionCarbohydrate = _ingredientRepository.GetIngredientById(id).IngredientNutrition.Carbohydrate;
                                    var ingredientNutritionCholesterol = _ingredientRepository.GetIngredientById(id).IngredientNutrition.Cholesterol;

                                    if (ingredientNutritionCalories != 0) {
                                        recipeNutrition.Calories += (int)(ingredientNutritionCalories * Quantities[i] / 2);
                                    }
                                    if (ingredientNutritionFat != 0) {
                                        recipeNutrition.Fat += (int)(ingredientNutritionFat * Quantities[i] / 2);
                                    }
                                    if (ingredientNutritionProtein != 0) {
                                        recipeNutrition.Protein += (int)(ingredientNutritionProtein * Quantities[i] / 2);
                                    }
                                    if (ingredientNutritionFibre != 0) {
                                        recipeNutrition.Fibre += (int)(ingredientNutritionFibre * Quantities[i] / 2);
                                    }
                                    if (ingredientNutritionCarbohydrate != 0) {
                                        recipeNutrition.Carbohydrate += (int)(ingredientNutritionCarbohydrate * Quantities[i] / 2);
                                    }
                                    if (ingredientNutritionCholesterol != 0) {
                                        recipeNutrition.Cholesterol += (int)(ingredientNutritionCholesterol * Quantities[i] / 2);
                                    }
                                } else {
                                    return NotFound();
                                }
                            }
                            recipe.Nutrition = recipeNutrition;
                            RemoveDuplicateIngredientsRecipe(ingredients);
                            RemoveDuplicateRecipeIngredients(recipeIngredients);
                            _recipeRepository.InsertRecipe(recipe, ingredients, recipeIngredients);
                            TempData["success"] = "The recipe has been submitted for review!";
                            return RedirectToAction(nameof(Index));
                        }
                    } else {
                        recipe.ImgPath = "untitle.jpg";
                    }
                }
                ViewData["FkRecipeId"] = new SelectList(_recipeRepository.GetRecipes(), "Id", "Id", recipe.FkRecipeId);
                ViewData["FkRecipeCategoryId"] = new SelectList(_recipeCategoryRepository.GetRecipeCategories(), "Id", "Name", recipe.FkRecipeCategoryId);
            }
            return View(recipe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Recipe recipe, string[] IngredientNames, double[] Quantities, string[] UnitOfMeasures) {
            if (ModelState.IsValid) {
                if (id != recipe.Id) {
                    return NotFound();
                }
                var existingRecipe = _recipeRepository.GetRecipeForEdit(id);
                _recipeRepository.SetValueForEdit(existingRecipe, recipe);
                if (recipe.file != null && recipe.file.Length > 0) {
                    IFormFile file = recipe.file;
                    // Generate a unique file name
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null) {
                        // Upload the file to Firebase Storage
                        string imageUrl = await UploadFirebase(file.OpenReadStream(), uniqueFileName);
                        Uri imageUrlUri = new(imageUrl);
                        string baseUrl = $"{imageUrlUri.GetLeftPart(UriPartial.Path)}?alt=media";
                        existingRecipe.ImgPath = baseUrl;
                    }
                }
                if (IngredientNames != null) {

                    var allIngredients = _ingredientRepository.GetStatusTrueIngredients();

                    var ingredients = allIngredients
                                      .Where(i => IngredientNames.Any(input => i.Name.Equals(input)))
                                      .ToList();

                    var recipeIngredients = new List<RecipeIngredient>();
                    Nutrition recipeNutrition = new Nutrition {
                        Calories = 0,
                        Fat = 0,
                        Protein = 0,
                        Fibre = 0,
                        Carbohydrate = 0,
                        Cholesterol = 0,
                    };

                    for (int i = 0; i < IngredientNames.Length; i++) {
                        bool exists = _ingredientRepository.GetStatusTrueIngredients()
                            .Any(ingredient => ingredient.Name.Equals(IngredientNames[i], StringComparison.OrdinalIgnoreCase));

                        if (exists) {
                            getIngredientIdFromName(IngredientNames[i], out int ingredientId);
                            recipeIngredients.Add(new RecipeIngredient {
                                IngredientId = ingredientId,
                                RecipeId = recipe.Id,
                                Quantity = Quantities[i],
                                UnitOfMeasure = UnitOfMeasures[i]
                            });

                            var ingredientNutritionCalories = _ingredientRepository.GetIngredientById(ingredientId).IngredientNutrition.Calories;
                            var ingredientNutritionFat = _ingredientRepository.GetIngredientById(ingredientId).IngredientNutrition.Fat;
                            var ingredientNutritionProtein = _ingredientRepository.GetIngredientById(ingredientId).IngredientNutrition.Protein;
                            var ingredientNutritionFibre = _ingredientRepository.GetIngredientById(ingredientId).IngredientNutrition.Fibre;
                            var ingredientNutritionCarbohydrate = _ingredientRepository.GetIngredientById(ingredientId).IngredientNutrition.Carbohydrate;
                            var ingredientNutritionCholesterol = _ingredientRepository.GetIngredientById(ingredientId).IngredientNutrition.Cholesterol;

                            if (ingredientNutritionCalories != 0) {
                                recipeNutrition.Calories += (int)(ingredientNutritionCalories * Quantities[i] / 2);
                            }
                            if (ingredientNutritionFat != 0) {
                                recipeNutrition.Fat += (int)(ingredientNutritionFat * Quantities[i] / 2);
                            }
                            if (ingredientNutritionProtein != 0) {
                                recipeNutrition.Protein += (int)(ingredientNutritionProtein * Quantities[i] / 2);
                            }
                            if (ingredientNutritionFibre != 0) {
                                recipeNutrition.Fibre += (int)(ingredientNutritionFibre * Quantities[i] / 2);
                            }
                            if (ingredientNutritionCarbohydrate != 0) {
                                recipeNutrition.Carbohydrate += (int)(ingredientNutritionCarbohydrate * Quantities[i] / 2);
                            }
                            if (ingredientNutritionCholesterol != 0) {
                                recipeNutrition.Cholesterol += (int)(ingredientNutritionCholesterol * Quantities[i] / 2);
                            }
                        } else {
                            return NotFound();
                        }
                    }

                    RemoveDuplicateIngredientsRecipe(ingredients);
                    RemoveDuplicateRecipeIngredients(recipeIngredients);
                    _recipeRepository.UpdateRecipe(existingRecipe, ingredients, recipeIngredients, recipeNutrition);
                }
                try {
                    _recipeRepository.SetStatusFalse(existingRecipe);
                } catch (DbUpdateConcurrencyException) {
                    if (!RecipeExists(recipe.Id)) {
                        return NotFound();
                    } else {
                        throw;
                    }
                }
                TempData["success"] = "The recipe has been submitted for review!";
                return RedirectToAction("MyRecipes", "Home");
            }

            return View(recipe);
        }
        public void getIngredientIdFromName(string name, out int id) {
            var ingredient = _ingredientRepository.GetIngredients()
                .FirstOrDefault(x => x.Name.Equals(name));
            id = ingredient.Id;
        }

        public void RemoveDuplicateRecipeIngredients(List<RecipeIngredient> recipeIngredients) {
            var distinctIngredients = new HashSet<int>();
            recipeIngredients.RemoveAll(ri => !distinctIngredients.Add((int)ri.IngredientId));
        }

        public void RemoveDuplicateIngredientsRecipe(List<Ingredient> ingredientsRecipe) {
            var distinctIngredients = new HashSet<int>();
            ingredientsRecipe.RemoveAll(ri => !distinctIngredients.Add((int)ri.Id));
        }

        // GET: Recipes/Edit/5
        [Authorize]
        public IActionResult Edit(int? id) {
            if (id == null || _recipeRepository.GetRecipes() == null) {
                return NotFound();
            }
            var recipe = _recipeRepository.GetRecipeForEdit(id);
            if (recipe == null) {
                return NotFound();
            }

            ViewData["FkRecipeId"] = new SelectList(_recipeRepository.GetRecipes(), "Id", "Id", recipe.FkRecipeId);
            ViewData["FkRecipeCategoryId"] = new SelectList(_recipeCategoryRepository.GetRecipeCategories(), "Id", "Name", recipe.FkRecipeCategoryId);
            ViewBag.PrepTime = new SelectList(GetPrepTimeList(), "Value", "Text", recipe.PrepTime);
            ViewBag.Difficulty = new SelectList(GetDifficultyList(), "Value", "Text", recipe.Difficult);
            foreach (var item in recipe.RecipeIngredients) {
                ViewData["Unit" + item.IngredientId] = new SelectList(GetUnitsofMeasureList(), "Value", "Text", item.UnitOfMeasure);
            }
            ViewBag.IngredientDetails = recipe.RecipeIngredients;
            ViewBag.Ingredients = recipe.Ingredients;
            return View(recipe);
        }



        // GET: Recipes/Delete/5
        [Authorize]
        public IActionResult Delete(int? id) {
            ViewBag.Title = "Create recipe";
            if (id == null || _recipeRepository.GetRecipes() == null) {
                return NotFound();
            }

            var recipe = _recipeRepository.GetRecipeForDelete(id);
            if (recipe == null) {
                return NotFound();
            }

            return View(recipe);
        }

        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id) {
            if (_recipeRepository.GetRecipes() == null) {
                return Problem("Entity set 'RecipeOrganizerContext.Recipes'  is null.");
            }
            var recipe = _recipeRepository.GetRecipeById(id);
            if (recipe != null) {
                _recipeRepository.DeleteRecipe(recipe);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool RecipeExists(int id) {
            return (_recipeRepository.GetRecipes()?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        public bool IngredientNameExists(string name) {
            return (_ingredientRepository.GetIngredients()?.Any(e => e.Name.Equals(name))).GetValueOrDefault();
        }

        //firebase
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

                using (MemoryStream webpStream = new MemoryStream()) {
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
                        .Child("recipes")
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