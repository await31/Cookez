using BusinessObjects.Models;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public class RecipeRepository : IRecipeRepository {
        public IEnumerable<Recipe> GetRecipes() => RecipeDAO.Instance.GetRecipes();
        public IEnumerable<Recipe> GetRecipesIndex() => RecipeDAO.Instance.GetRecipesIndex();
        public IEnumerable<Recipe> GetRecipesHomeIndex() => RecipeDAO.Instance.GetRecipesHomeIndex();
        public IEnumerable<Recipe> GetRecipesUserProfile(string? userId) => RecipeDAO.Instance.GetRecipesUserProfile(userId);
        public IEnumerable<Recipe> GetRecipesMyRecipes(Account acc) => RecipeDAO.Instance.GetRecipesMyRecipes(acc);
        public IEnumerable<Recipe> GetStatusFalseRecipes() => RecipeDAO.Instance.GetStatusFalseRecipes();
        public IEnumerable<Recipe> GetStatusTrueRecipes() => RecipeDAO.Instance.GetStatusTrueRecipes();
        public Recipe GetHotRecipe() => RecipeDAO.Instance.GetHotRecipe();
        public Recipe GetRecipeById(int? id) => RecipeDAO.Instance.GetRecipeById(id);
        public Recipe GetRecipeForEdit(int? id) => RecipeDAO.Instance.GetRecipeForEdit(id);
        public Recipe GetRecipeByIdForFavourite(int? id) => RecipeDAO.Instance.GetRecipeByIdForFavourite(id);
        public Recipe GetRecipeForDetails(int recipeId) => RecipeDAO.Instance.GetRecipeForDetails(recipeId);
        public Recipe GetRecipeForHomeRecipeDetails(int? id) => RecipeDAO.Instance.GetRecipeForHomeRecipeDetails(id);
        public Recipe GetRecipeForDelete(int? recipeId) => RecipeDAO.Instance.GetRecipeForDelete(recipeId);
        public IEnumerable<Recipe> GetSuggestRecipes(int currentRecipeId) => RecipeDAO.Instance.GetSuggestRecipes(currentRecipeId);
        public void UpdateRecipe(Recipe existingRecipe, List<Ingredient> list, List<RecipeIngredient> ris, Nutrition recipeNutrition) => RecipeDAO.Instance.UpdateRecipe(existingRecipe, list, ris, recipeNutrition);
        public void InsertRecipe(Recipe r, List<Ingredient> list, List<RecipeIngredient> ris) => RecipeDAO.Instance.InsertRecipe(r, list, ris);
        public void InsertRecipeIngredient(List<RecipeIngredient> ris) => RecipeDAO.Instance.InsertRecipeIngredient(ris);
        public void InsertRecipeIngredientToRecipe(Recipe r, List<RecipeIngredient> ris) => RecipeDAO.Instance.InsertRecipeIngredientToRecipe(r, ris);
        public void DeleteRecipe(Recipe r) => RecipeDAO.Instance.DeleteRecipe(r);
        public void Approve(Recipe r) => RecipeDAO.Instance.Approve(r);
        public void Deny(Recipe r) => RecipeDAO.Instance.Deny(r);
        public void SetValueForEdit(Recipe existingRecipe, Recipe recipe) => RecipeDAO.Instance.SetValueForEdit(existingRecipe, recipe);
        public void SetStatusFalse(Recipe r) => RecipeDAO.Instance.SetStatusFalse(r);
    }
}
