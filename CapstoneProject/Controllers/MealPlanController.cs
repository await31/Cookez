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
using System.Text.Json;
using System.Drawing.Printing;
using System.Globalization;
using System.Text.RegularExpressions;
using Repositories;
using BusinessObjects.Models;
using System.Text.Json.Serialization;

namespace CapstoneProject.Controllers {

    [Authorize]
    public class MealPlanController : Controller {

        private readonly UserManager<Account> _userManager;
        private readonly IMealPlanRepository _mealPlanRepository;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IRecipeCategoryRepository _recipeCategoryRepository;
        private readonly IAccountRepository _accountRepository;

        public MealPlanController(IAccountRepository accountRepository, IRecipeCategoryRepository recipeCategoryRepository, IMealPlanRepository mealPlanRepository, IRecipeRepository recipeRepository, UserManager<Account> userManager) {
            _mealPlanRepository = mealPlanRepository;
            _recipeRepository = recipeRepository;
            _recipeCategoryRepository = recipeCategoryRepository;
            _accountRepository = accountRepository;
            _userManager = userManager;
        }


        public JsonResult GetEvents() {
            var currentUser = _userManager.GetUserId(User);
            var mealplans = _mealPlanRepository.GetMealPlans(currentUser)
                .ToList();

            return Json(mealplans);
        }

        [HttpPost]
        public JsonResult RecipeNameExists(string recipe) {
            ExtractIntegerAndString(recipe, out int id, out string name);
            var exists = _recipeRepository.GetRecipes()?.Any(r => r.Id == id && r.Name.Equals(name)) ?? false;
            return Json(new { exists });
        }

        [HttpPost]
        public JsonResult GetDietaryRecipes(string dietary) {
            // Generate data based on the selected dietary
            var allRecipes = _recipeRepository.GetRecipes().Where(a=>a.Status ==true);

            IEnumerable<Recipe> recipes = new List<Recipe>();
            switch (dietary) {
                case "highcalorie":
                    recipes = allRecipes.Where(a=>a.Nutrition.Calories!= null).OrderByDescending(a=>a.Nutrition.Calories).Take(8).ToList();
                    break;
                case "lowcalorie":
                    recipes = allRecipes.Where(a => a.Nutrition.Calories != null).OrderBy(a => a.Nutrition.Calories).Take(8).ToList();
                    break;
                case "highprotein":
                    recipes = allRecipes.Where(a => a.Nutrition.Protein != null).OrderByDescending(a => a.Nutrition.Protein).Take(8).ToList();
                    break;
                case "lowprotein":
                    recipes = allRecipes.Where(a => a.Nutrition.Protein != null).OrderBy(a => a.Nutrition.Protein).Take(8).ToList();
                    break;
                case "highfibre":
                    recipes = allRecipes.Where(a => a.Nutrition.Fibre != null).OrderByDescending(a => a.Nutrition.Fibre).Take(8).ToList();
                    break;
                case "lowfibre":
                    recipes = allRecipes.Where(a => a.Nutrition.Fibre != null).OrderBy(a => a.Nutrition.Fibre).Take(8).ToList();
                    break;
                case "highcarb":
                    recipes = allRecipes.Where(a => a.Nutrition.Carbohydrate != null).OrderByDescending(a => a.Nutrition.Carbohydrate).Take(8).ToList();
                    break;
                case "lowcarb":
                    recipes = allRecipes.Where(a => a.Nutrition.Carbohydrate != null).OrderBy(a => a.Nutrition.Carbohydrate).Take(8).ToList();
                    break;
                case "lowfat":
                    recipes = allRecipes.Where(a => a.Nutrition.Fat != null).OrderBy(a => a.Nutrition.Fat).Take(8).ToList();
                    break;
                case "highfat":
                    recipes = allRecipes.Where(a => a.Nutrition.Fat != null).OrderByDescending(a => a.Nutrition.Fat).Take(8).ToList();
                    break;
            }
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true
            };
            return Json(recipes, options);
        }

        public static void ExtractIntegerAndString(string inputString, out int integer, out string stringValue) {
            Match integerMatch = Regex.Match(inputString, @"\#(\d+)");
            if (integerMatch.Success) {
                integer = int.Parse(integerMatch.Groups[1].Value);
            } else {
                integer = 0;
            }
            string strippedString = Regex.Replace(inputString, @"\(\#\d+\)", "").Trim();
            stringValue = strippedString;
        }

        [HttpPost]
        public JsonResult RecipesAutoComplete(string term) {
            var result = _recipeRepository.GetRecipes()
                .Where(t => t.Name.ToLower().Contains(term.ToLower()))
                .Select(t => new { Id = t.Id, Name = t.Name })
                .ToList();

            return Json(result);
        }

        [Breadcrumb("My Planning")]
        public IActionResult Index(int pg = 1) {
            var currentUser = _userManager.GetUserId(User);
            var dateNow = DateTime.Now;
            var startOfWeek = dateNow.Date.AddDays(-(int)dateNow.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);

            var mealPlans = _mealPlanRepository.GetMealPlans(currentUser)
                .Where(a => a.Date >= startOfWeek && a.Date <= endOfWeek)
            .ToList();

            var count = mealPlans.Count;
            ViewData["count"] = count;

            return View(mealPlans);
        }

        [Breadcrumb("Schedule")]
        public IActionResult Schedule() {
            var currentUser = _userManager.GetUserId(User);
            var mealPlans = _mealPlanRepository.GetMealPlans(currentUser)
                .ToList();
            return View(mealPlans);
        }

        [Breadcrumb("Details")]
        public IActionResult Details(int? id) {

            var allMealPlan = _mealPlanRepository.GetAllMealPlans();

            var mealplan = _mealPlanRepository.GetMealPlanDetails(id);

            if (mealplan == null) {
                return NotFound();
            }
            var mealPlanWithNutrition = _mealPlanRepository.GetMealPlanWithNutrition(id);
            var ingredientQuantities = mealplan.Recipes
        .SelectMany(r => r.RecipeIngredients)
        .GroupBy(ri => new { ri.IngredientId, ri.UnitOfMeasure })
        .Select(group => new RecipeIngredient {
            IngredientId = group.Key.IngredientId,
            Ingredient = group.FirstOrDefault().Ingredient,
            UnitOfMeasure = group.Key.UnitOfMeasure,
            Quantity = group.Sum(ri => ri.Quantity)
        })
        .ToList();

            var totalNutrition = new Nutrition() {
                Calories = mealPlanWithNutrition.Recipes.Sum(r => r.Nutrition.Calories),
                Fat = mealPlanWithNutrition.Recipes.Sum(r => r.Nutrition.Fat),
                Protein = mealPlanWithNutrition.Recipes.Sum(r => r.Nutrition.Protein),
                Fibre = mealPlanWithNutrition.Recipes.Sum(r=> r.Nutrition.Fibre),
                Carbohydrate = mealPlanWithNutrition.Recipes.Sum(r => r.Nutrition.Carbohydrate),
                Cholesterol = mealPlanWithNutrition.Recipes.Sum(r => r.Nutrition.Cholesterol)
            };
            ViewData["TotalNutrition"] = totalNutrition;
            ViewData["GroupedIngredients"] = ingredientQuantities;
            return View(mealplan);
        }

        [Breadcrumb("Create")]
        public IActionResult Create() {
            var recipes = _recipeRepository.GetRecipes()
                       .Where(i => i.Status == true)
                       .ToList();

            var categories = _recipeCategoryRepository.GetRecipeCategories()
                .ToList();

            ViewData["categories"] = categories;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MealPlan mealplan, string[] RecipeNames, DateTime startDate, string[] selectedDays, int weeklast) {
            if (ModelState.IsValid) {
                var currentUser = await _userManager.GetUserAsync(User);
                var allRecipes = _recipeRepository.GetRecipes()
                    .ToList();

                if (currentUser != null && RecipeNames != null) {
                    if (mealplan.Date != null) {  //Non Weekly Planning
                        List<int> recipeIds = new List<int>();
                        foreach (var recipeName in RecipeNames) {
                            ExtractIntegerAndString(recipeName, out int id, out string name);
                            recipeIds.Add(id);
                        }
                        _mealPlanRepository.InsertMealPlan(mealplan,currentUser.Id,recipeIds);
                    } else if (mealplan.Date == null) {  //Weekly planning

                        // Process selected days and week last
                        if (selectedDays != null && selectedDays.Any()) {

                            var selectedDayIndexes = new List<int>();

                            // Map selected days to their respective indexes (0 for Sunday, 1 for Monday, etc.)
                            var dayNames = Enum.GetNames(typeof(DayOfWeek)).ToList();

                            foreach (var selectedDay in selectedDays) {
                                var index = dayNames.FindIndex(d => string.Equals(d, selectedDay, StringComparison.OrdinalIgnoreCase));
                                if (index >= 0) {
                                    selectedDayIndexes.Add(index);
                                }
                            }

                            // Generate meal plans for each selected day in the specified week range
                            for (int weekOffset = 1; weekOffset <= weeklast; weekOffset++) {

                                foreach (var selectedDayIndex in selectedDayIndexes) {
                                    var targetDate = GetNextOccurrenceOfDay(startDate, (DayOfWeek)selectedDayIndex, weekOffset);

                                    var newMealPlan = new MealPlan {
                                        Date = targetDate,
                                        Title = mealplan.Title,
                                        Description = mealplan.Description,
                                        Color = mealplan.Color
                                    };
                                    var recipeIds = new List<int>();
                                    foreach (var recipeName in RecipeNames) {
                                        ExtractIntegerAndString(recipeName, out int id, out string name);
                                        recipeIds.Add(id);
                                    }
                                    _mealPlanRepository.InsertMealPlan(newMealPlan,currentUser.Id,recipeIds);
                                }
                            }
                        }
                    }
                    TempData["success"] = "Meal planning created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            TempData["error"] = "Sorry, something went wrong!";
            return View();
        }

        [HttpPost]
        public IActionResult DeletePOST(int? id) {
            var obj = _mealPlanRepository.GetMealPlanById(id);
            if (obj == null) {
                return NotFound();
            }

            _mealPlanRepository.DeleteMealPlan(obj);
            TempData["success"] = "Meal plan deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        static DateTime GetNextOccurrenceOfDay(DateTime date, DayOfWeek selectedDay, int weekOffset) {
            int daysToAdd = (int)selectedDay - (int)date.DayOfWeek;
            if (daysToAdd < 0) {
                daysToAdd += 7; // Add 7 days to get the next occurrence
            }

            var nextOccurrence = date.AddDays(daysToAdd).AddDays(7 * (weekOffset - 1));
            return nextOccurrence;
        }
    }
}