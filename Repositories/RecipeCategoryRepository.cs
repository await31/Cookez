using BusinessObjects.Models;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public class RecipeCategoryRepository : IRecipeCategoryRepository {
        public IEnumerable<RecipeCategory> GetRecipeCategories() => RecipeCategoryDAO.Instance.GetRecipeCategories();
        public RecipeCategory GetRecipeCategoryById(int? id) => RecipeCategoryDAO.Instance.GetRecipeCategoryById(id);
        public void InsertRecipeCategory(RecipeCategory rc) => RecipeCategoryDAO.Instance.InsertRecipeCategory(rc);
        public void UpdateRecipeCategory(RecipeCategory rc) => RecipeCategoryDAO.Instance.UpdateRecipeCategory(rc);
        public void DeleteRecipeCategory(RecipeCategory rc) => RecipeCategoryDAO.Instance.DeleteRecipeCategory(rc);
    }
}
