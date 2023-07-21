using BusinessObjects.Models;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public class MealPlanRepository : IMealPlanRepository {
        public IEnumerable<MealPlan> GetAllMealPlans() => MealPlanDAO.Instance.GetAllMealPlans();
        public IEnumerable<MealPlan> GetMealPlans(string? accId) => MealPlanDAO.Instance.GetMealPlans(accId);
        public MealPlan GetMealPlanById(int? id) => MealPlanDAO.Instance.GetMealPlanById(id);
        public MealPlan GetMealPlanDetails(int? id) => MealPlanDAO.Instance.GetMealPlanDetails(id);
        public MealPlan GetMealPlanWithNutrition(int? id) => MealPlanDAO.Instance.GetMealPlanWithNutrition(id);
        public void InsertMealPlan(MealPlan mp, string userId, List<int> recipeIds) => MealPlanDAO.Instance.InsertMealPlan(mp,userId,recipeIds);
        public void DeleteMealPlan(MealPlan mp) => MealPlanDAO.Instance.DeleteMealPlan(mp);

    }
}
