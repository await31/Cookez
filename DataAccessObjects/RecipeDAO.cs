using BusinessObjects.Models;
using Firebase.Auth;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessObjects {
    public class RecipeDAO {
        private static RecipeDAO instance = null;
        private static readonly object instacelock = new object();
        private RecipeDAO() { }
        public static RecipeDAO Instance {
            get {
                lock (instacelock) {
                    if (instance == null) {
                        instance = new RecipeDAO();
                    }
                    return instance;
                }
            }
        }

        public List<Recipe> GetRecipes() {
            List<Recipe> lists = new List<Recipe>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Recipes
                .Where(a => a.Status == true)
                .Include(x => x.FkRecipeCategory)
                .Include(x => x.FkUser)
                .Include(x => x.Nutrition)
                .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public List<Recipe> GetStatusFalseRecipes() {
            List<Recipe> lists = new List<Recipe>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Recipes
                .Where(a => a.Status == false)
                .Include(x => x.FkUser)
                .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public List<Recipe> GetStatusTrueRecipes() {
            List<Recipe> lists = new List<Recipe>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Recipes
                .Where(a => a.Status == true)
                .Include(x => x.FkUser)
                .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public List<Recipe> GetRecipesHomeIndex() {
            List<Recipe> lists = new List<Recipe>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Recipes
                            .Where(a => a.Status == true)
                            .Include(r => r.FkRecipeCategory)
                            .OrderByDescending(b => b.CreatedDate)
                            .Take(8)
                            .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public List<Recipe> GetRecipesUserProfile(string? userId) {
            List<Recipe> lists = new List<Recipe>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Recipes
                .Where(a => a.Status == true)
                .Where(a =>a.FkUserId == userId)
                .Include(r => r.FkRecipe)
                .Include(r => r.FkRecipeCategory)
                .Include(r => r.FkUser)
                .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public List<Recipe> GetRecipesMyRecipes(Account acc) {
            List<Recipe> lists = new List<Recipe>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Recipes
                .Where(a => a.Status == true && a.FkUser == acc)
                .Include(r => r.FkRecipe)
                .Include(r => r.FkRecipeCategory)
                .Include(r => r.FkUser).ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public Recipe GetHotRecipe() {
            Recipe recipe = new Recipe();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    recipe = context.Recipes
                            .Where(a => a.Status == true)
                            .Include(r => r.FkRecipeCategory)
                            .Include(b => b.FkUser)
                            .OrderByDescending(a => a.ViewCount)
                            .FirstOrDefault();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return recipe;
        }

        public Recipe GetRecipeForHomeRecipeDetails(int? id) {
            Recipe recipe = new Recipe();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    recipe = context.Recipes
                         .Include(x => x.FkUser)
                         .Include(y => y.FkRecipeCategory)
                         .Include(z => z.Ingredients)
                         .Include(t => t.Nutrition)
                         .Include(a => a.RecipeIngredients)
                            .ThenInclude(a => a.Ingredient)
                         .Where(Ingredient => Ingredient.Status == true)
                         .FirstOrDefault(a => a.Id == id);
                    recipe.ViewCount++;
                    context.SaveChanges();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return recipe;
        }

        public Recipe GetRecipeForDetails(int recipeId) {
            Recipe recipe = new Recipe();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    recipe = context.Recipes
               .Where(a => a.Status == true)
               .Include(x => x.FkUser)
               .Include(y => y.FkRecipeCategory)
               .Include(t => t.Nutrition)
               .Include(a => a.RecipeIngredients)
               .ThenInclude(a => a.Ingredient)
               .Where(Ingredient => Ingredient.Status == true)
               .FirstOrDefault(a => a.Id == recipeId);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return recipe;
        }

        public Recipe GetRecipeForDelete(int? recipeId) {
            Recipe recipe = new Recipe();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    recipe = context.Recipes
                .Include(r => r.FkRecipe)
                .Include(r => r.FkRecipeCategory)
                .FirstOrDefault(m => m.Id == recipeId);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return recipe;
        }

        public List<Recipe> GetSuggestRecipes(int currentRecipeId) {
            List<Recipe> suggestRecipes = new List<Recipe>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    suggestRecipes = context.Recipes
               .Where(a => a.Status == true)
               .Where(a => a.Id != currentRecipeId)
               .Include(a => a.FkRecipeCategory)
               .OrderByDescending(x => x.ViewCount)
               .Take(4)
               .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return suggestRecipes;
        }

        public List<Recipe> GetRecipesIndex() {
            List<Recipe> lists = new List<Recipe>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Recipes
                .Where(a => a.Status == true)
                .Include(a => a.RecipeIngredients)
                .Include(a => a.FkRecipeCategory)
                .Include(b => b.Ingredients).ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public Recipe GetRecipeById(int? id) {
            Recipe r = new();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    r = context.Recipes
                .Include(x => x.FkUser)
                .Include(y => y.FkRecipeCategory)
                .Include(t => t.Nutrition)
                .Include(a => a.RecipeIngredients)
                .ThenInclude(a => a.Ingredient)
                .SingleOrDefault(m => m.Id == id);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return r;
        }

        public Recipe GetRecipeByIdForFavourite(int? id) {
            Recipe r = new();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    r = context.Recipes
                .FirstOrDefault(m => m.Id == id);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return r;
        }

        public Recipe GetRecipeForEdit(int? id) {
            Recipe r = new();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    r = context.Recipes
                        .Include(r => r.RecipeIngredients)
                        .Include(r => r.Ingredients)
                        .Include(r => r.Nutrition)
                        .Single(r => r.Id == id);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return r;
        }

        public void SetValueForEdit(Recipe existingRecipe, Recipe recipe) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetRecipeById(recipe.Id);
                    if (cus != null) {
                        context.Entry(existingRecipe).CurrentValues.SetValues(recipe);
                    } else {
                        throw new Exception("Recipe is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void InsertRecipe(Recipe r, List<Ingredient> list, List<RecipeIngredient> ris) {
            using (var context = new RecipeOrganizerContext()) {
                var existingIngredients = context.Ingredients
                    .Where(i => list.Select(x => x.Id).Contains(i.Id))
                    .ToList();

                r.Ingredients = existingIngredients;

                foreach (var ri in ris) {
                    ri.RecipeId = r.Id;
                }

                r.RecipeIngredients = ris;

                context.Recipes.Add(r);
                context.SaveChanges();
            }
        }

        public void UpdateRecipe(Recipe existingRecipe, List<Ingredient> list, List<RecipeIngredient> ris, Nutrition recipeNutrition) {
            using (var context = new RecipeOrganizerContext()) {
                if (existingRecipe != null) {

                    // Attach existingRecipe to the context
                    context.Recipes.Attach(existingRecipe);

                    // Remove existing RecipeIngredients associated with the existingRecipe
                    var existingRecipeIngredients = context.RecipeIngredient
                        .Where(a => a.RecipeId == existingRecipe.Id)
                        .ToList();

                    context.RecipeIngredient.RemoveRange(existingRecipeIngredients);

                    var existingIngredients = context.Ingredients
                        .Where(i => list.Select(x => x.Id).Contains(i.Id))
                        .ToList();

                    existingRecipe.Ingredients.Clear();
                    context.SaveChanges();

                    existingRecipe.Ingredients = existingIngredients;
                    context.SaveChanges();

                    // Set the RecipeId for the RecipeIngredients
                    foreach (var ri in ris) {
                        ri.RecipeId = existingRecipe.Id;
                    }

                    // Update RecipeIngredients
                    existingRecipe.RecipeIngredients = ris;

                    // Update Nutrition
                    existingRecipe.Nutrition.Calories = recipeNutrition.Calories;
                    existingRecipe.Nutrition.Fat = recipeNutrition.Fat;
                    existingRecipe.Nutrition.Fibre = recipeNutrition.Fibre;
                    existingRecipe.Nutrition.Carbohydrate = recipeNutrition.Carbohydrate;
                    existingRecipe.Nutrition.Cholesterol = recipeNutrition.Cholesterol;
                    existingRecipe.Nutrition.Protein = recipeNutrition.Protein;

                    context.Recipes.Update(existingRecipe);
                    context.SaveChanges();
                }
            }
        }



        public void InsertRecipeIngredientToRecipe(Recipe r, List<RecipeIngredient> ris) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetRecipeById(r.Id);
                    if (cus != null) {
                        r.RecipeIngredients.AddRange(ris);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Recipe is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void InsertRecipeIngredient(List<RecipeIngredient> ris) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    context.RecipeIngredient.AddRange(ris);
                    context.SaveChanges();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteRecipe(Recipe r) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetRecipeById(r.Id);
                    if (cus != null) {

                        var recipeIngredient = context.RecipeIngredient
                             .Where(a => a.RecipeId == r.Id)
                             .ToList();
                        context.RecipeIngredient.RemoveRange(recipeIngredient);

                        var recipeFeedbacks = context.RecipeFeedbacks
                            .Where(a => a.RecipeId == r.Id)
                            .ToList();
                        context.RecipeFeedbacks.RemoveRange(recipeFeedbacks);
                        context.Recipes.Remove(r);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Recipe is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void Deny(Recipe r) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetRecipeById(r.Id);
                    if (cus != null) {
                        var recipeIngredient = context.RecipeIngredient
                            .Where(r => r.RecipeId == r.Id)
                            .ToList();
                        context.RecipeIngredient.RemoveRange(recipeIngredient);
                        context.Recipes.Remove(r);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Recipe is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void Approve(Recipe r) {
            using (var context = new RecipeOrganizerContext()) {
                var recipe = context.Recipes.FirstOrDefault(recipe => recipe.Id == r.Id);
                if (recipe != null) {
                    recipe.Status = true; // Set the status to approved
                    context.SaveChanges();
                } else {
                    throw new Exception("Recipe does not exist.");
                }
            }
        }

        public void SetStatusFalse(Recipe r) {
            using (var context = new RecipeOrganizerContext()) {
                var recipe = context.Recipes.FirstOrDefault(recipe => recipe.Id == r.Id);
                if (recipe != null) {
                    recipe.Status = false;
                    context.SaveChanges();
                } else {
                    throw new Exception("Recipe does not exist.");
                }
            }
        }
    }
}
