using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;
using System.Data;
using BusinessObjects.Models;
using Repositories;

namespace CapstoneProject.Controllers {

    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller {

        private readonly IRecipeRepository _recipeRepository;
        private readonly IRecipeCategoryRepository _recipeCategoryRepository;
        private readonly IIngredientRepository _ingredientRepository;
        private readonly IIngredientCategoryRepository _ingredientCategoryRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IAccountRepository _accountRepository;


        public DashboardController(IRecipeCategoryRepository recipeCategoryRepository, IIngredientCategoryRepository ingredientCategoryRepository, IAccountRepository accountRepository, IRecipeRepository recipeRepository, IIngredientRepository ingredientRepository, IContactRepository contactRepository) {
            _recipeRepository = recipeRepository;
            _ingredientRepository = ingredientRepository;
            _contactRepository = contactRepository;
            _accountRepository = accountRepository;
            _recipeCategoryRepository = recipeCategoryRepository;
            _ingredientCategoryRepository = ingredientCategoryRepository;
        }


        [Breadcrumb("Dashboard")]
        public IActionResult Index() {
            IEnumerable<Recipe> objRecipe = _recipeRepository.GetStatusFalseRecipes();

            IEnumerable<Ingredient> pendingIngredients = _ingredientRepository.GetStatusFalseIngredients();

            IEnumerable<Contact> contact = _contactRepository.GetContacts();

            IEnumerable<Account> accounts = _accountRepository.GetAccounts();


            //Get counts of ingredients, recipes, and pending request
            ViewData["Contact"] = contact;
            ViewData["accCount"] = accounts.Count();
            ViewData["IngredientsCount"] = _ingredientRepository.GetIngredients()
                .Count();
            ViewData["RecipesCount"] = _recipeRepository.GetStatusTrueRecipes()
                .Count();
            ViewData["PendingRequestsCount"] = objRecipe
                .Count();
            ViewData["IngredientsPending"] = pendingIngredients;

            //chart 

            //Ingredient pie chart data
            var ingredientCounts = _ingredientRepository.GetStatusTrueIngredients()
              .Join(
                  _ingredientCategoryRepository.GetIngredientCategories(),
                  ingredient => ingredient.FkCategoryId,
                  category => category.Id,
                  (ingredient, category) => new { Ingredient = ingredient, Category = category }
              )
              .GroupBy(
                  pair => new { CategoryName = pair.Category.Name },
                  (key, group) => new
                  {
                      CategoryName = key.CategoryName,
                      IngredientCount = group.Count()
                  }
              )
              .ToList();

            var ingredientChartData = new List<Object>();
            foreach (var item in ingredientCounts)
            {
                ingredientChartData.Add(new { Label = $"{item.CategoryName}", Value = item.IngredientCount });
            }
            ViewBag.IngredientChartData = Newtonsoft.Json.JsonConvert.SerializeObject(ingredientChartData);

            //Recipe doughnut chart data
            var recipeCounts = _recipeRepository.GetStatusTrueRecipes()
                .Join(
                    _recipeCategoryRepository.GetRecipeCategories(),
                    recipe => recipe.FkRecipeCategoryId,
                    category => category.Id,
                    (recipe, category) => new { Recipe = recipe, Category = category }
                )
                .GroupBy(
                    pair => new { CategoryName = pair.Category.Name },
                    (key, group) => new
                    {
                        CategoryName = key.CategoryName,
                        RecipesCount = group.Count()
                    }
                )
                .ToList();
            var recipeChartData = new List<Object>();
            foreach (var item in recipeCounts)
            {
                recipeChartData.Add(new { Label = $"{item.CategoryName}", Value = item.RecipesCount });
            }
            ViewBag.RecipeChartData = Newtonsoft.Json.JsonConvert.SerializeObject(recipeChartData);

            //Recipe line chart daata

            var recipeMonth = _recipeRepository.GetStatusTrueRecipes()
                .GroupBy(r => r.CreatedDate.Value.Month)
                 .Select(g => new
                 {
                     MonthNumber = g.Key,
                     RecipeCount = g.Count()
                 })
                .OrderBy(g => g.MonthNumber)
                .ToList();

            var recipeMonthChartData = new int[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (var item in recipeMonth)
            {
                recipeMonthChartData[item.MonthNumber - 1] = item.RecipeCount;

            }
            var labels = new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            ViewBag.RecipeLineChartLabels = Newtonsoft.Json.JsonConvert.SerializeObject(labels);
            ViewBag.RecipeLineChartData = Newtonsoft.Json.JsonConvert.SerializeObject(recipeMonthChartData);
            return View(objRecipe);
        }
    }
}
