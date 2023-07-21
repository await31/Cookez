using BusinessObjects.Models;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public class IngredientRepository : IIngredientRepository {
        public IEnumerable<Ingredient> GetIngredients() => IngredientDAO.Instance.GetIngredients();
        public IEnumerable<Ingredient> GetStatusTrueIngredients() => IngredientDAO.Instance.GetStatusTrueIngredients();
        public IEnumerable<Ingredient> GetStatusFalseIngredients() => IngredientDAO.Instance.GetStatusFalseIngredients();
        public Ingredient GetIngredientById(int? id) => IngredientDAO.Instance.GetIngredientById(id);
        public void InsertIngredient(Ingredient i) => IngredientDAO.Instance.InsertIngredient(i);
        public void UpdateIngredient(Ingredient i) => IngredientDAO.Instance.UpdateIngredient(i);
        public void DeleteIngredient(Ingredient i) => IngredientDAO.Instance.DeleteIngredient(i);
        public void Approve(Ingredient i) => IngredientDAO.Instance.Approve(i);
        public void Deny(Ingredient i) => IngredientDAO.Instance.Deny(i);

    }
}
