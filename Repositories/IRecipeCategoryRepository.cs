using BusinessObjects.Models;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public interface IRecipeCategoryRepository {
        IEnumerable<RecipeCategory> GetRecipeCategories();
        RecipeCategory GetRecipeCategoryById(int? id);
        void InsertRecipeCategory(RecipeCategory ic);
        void UpdateRecipeCategory(RecipeCategory ic);
        void DeleteRecipeCategory(RecipeCategory ic);
    }
}
