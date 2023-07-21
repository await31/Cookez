using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public interface IIngredientRepository {
        IEnumerable<Ingredient> GetIngredients();
        IEnumerable<Ingredient> GetStatusTrueIngredients();
        IEnumerable<Ingredient> GetStatusFalseIngredients();
        Ingredient GetIngredientById(int? id);
        void InsertIngredient(Ingredient i);
        void DeleteIngredient(Ingredient i);
        void UpdateIngredient(Ingredient i);
        void Approve(Ingredient i);
        void Deny(Ingredient i);
    }
}
