using BusinessObjects.Models;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public interface IMealPlanRepository {
        IEnumerable<MealPlan> GetAllMealPlans();
        IEnumerable<MealPlan> GetMealPlans(string? accId);
        MealPlan GetMealPlanById(int? id);
        MealPlan GetMealPlanDetails(int? id);
        MealPlan GetMealPlanWithNutrition(int? id);
        void InsertMealPlan(MealPlan mp, string userId, List<int> recipeIds);
        void DeleteMealPlan(MealPlan mp);
    }
}
