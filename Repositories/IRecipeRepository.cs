using BusinessObjects.Models;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public interface IRecipeRepository {
        IEnumerable<Recipe> GetRecipes();
        IEnumerable<Recipe> GetRecipesIndex();
        Recipe GetRecipeById(int? id);
        IEnumerable<Recipe> GetRecipesHomeIndex();
        IEnumerable<Recipe> GetStatusFalseRecipes();
        IEnumerable<Recipe> GetStatusTrueRecipes();
        IEnumerable<Recipe> GetRecipesUserProfile(string? userId);
        IEnumerable<Recipe> GetRecipesMyRecipes(Account acc);
        Recipe GetHotRecipe();
        Recipe GetRecipeForEdit(int? id);
        Recipe GetRecipeByIdForFavourite(int? id);
        Recipe GetRecipeForDetails(int recipeId);
        Recipe GetRecipeForHomeRecipeDetails(int? id);
        Recipe GetRecipeForDelete(int? recipeId);
        IEnumerable<Recipe> GetSuggestRecipes(int currentRecipeId);
        void InsertRecipe(Recipe r, List<Ingredient> list, List<RecipeIngredient> ris);
        void InsertRecipeIngredient(List<RecipeIngredient> ris);
        void UpdateRecipe(Recipe existingRecipe, List<Ingredient> list, List<RecipeIngredient> ris, Nutrition recipeNutrition);
        void InsertRecipeIngredientToRecipe(Recipe r, List<RecipeIngredient> ris);
        void DeleteRecipe(Recipe r);
        void Approve(Recipe r);
        void Deny(Recipe r);
        void SetValueForEdit(Recipe existingRecipe, Recipe recipe);
        void SetStatusFalse(Recipe r);
    }
}
