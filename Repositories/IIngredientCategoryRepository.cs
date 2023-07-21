using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using DataAccessObjects;

namespace Repositories {
    public interface IIngredientCategoryRepository {
        IEnumerable<IngredientCategory> GetIngredientCategories();
        IngredientCategory GetIngredientCategoryById(int? id);
        void InsertIngredientCategory(IngredientCategory ic);
        void DeleteIngredientCategory(IngredientCategory ic);
        void UpdateIngredientCategory(IngredientCategory ic);

    }
}
