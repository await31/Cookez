using BusinessObjects.Models;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public class IngredientCategoryRepository : IIngredientCategoryRepository {
        public IEnumerable<IngredientCategory> GetIngredientCategories() => IngredientCategoryDAO.Instance.GetIngredientCategories();
        public IngredientCategory GetIngredientCategoryById(int? id) => IngredientCategoryDAO.Instance.GetIngredientCategoryById(id);
        public void InsertIngredientCategory(IngredientCategory ic) => IngredientCategoryDAO.Instance.InsertIngredientCategory(ic);
        public void UpdateIngredientCategory(IngredientCategory ic) => IngredientCategoryDAO.Instance.UpdateIngredientCategory(ic);
        public void DeleteIngredientCategory(IngredientCategory ic) => IngredientCategoryDAO.Instance.DeleteIngredientCategory(ic);

    }
}
