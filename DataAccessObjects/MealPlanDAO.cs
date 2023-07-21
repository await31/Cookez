using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataAccessObjects {
    public class MealPlanDAO {
        private static MealPlanDAO instance = null;
        private static readonly object instacelock = new object();
        private MealPlanDAO() { }
        public static MealPlanDAO Instance {
            get {
                lock (instacelock) {
                    if (instance == null) {
                        instance = new MealPlanDAO();
                    }
                    return instance;
                }
            }
        }
        public List<MealPlan> GetAllMealPlans() {
            List<MealPlan> list = new List<MealPlan>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    list = context.MealPlans
                        .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return list;
        }

        public List<MealPlan> GetMealPlans(string? accId) {
            List<MealPlan> list = new List<MealPlan>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    list = context.MealPlans
                        .Where(a => a.FkUserId == accId)
                        .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return list;
        }

        public MealPlan GetMealPlanById(int? id) {
            MealPlan mp = new();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    mp = context.MealPlans.SingleOrDefault(a => a.Id == id);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return mp;
        }

        public MealPlan GetMealPlanDetails(int? id) {
            MealPlan mp = new();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    mp = context.MealPlans
                       .Include(mp => mp.FkUser)
                       .Include(mp => mp.Recipes)
                            .ThenInclude(r => r.FkUser)
                       .Include(mp => mp.Recipes)
                            .ThenInclude(r => r.RecipeIngredients)
                                 .ThenInclude(ri => ri.Ingredient)
                       .FirstOrDefault(mp => mp.Id == id);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return mp;
        }

        public MealPlan GetMealPlanWithNutrition(int? id) {
            MealPlan mp = new();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    mp = context.MealPlans
                .Include(r => r.Recipes)
                .ThenInclude(r => r.Nutrition)
                .FirstOrDefault(a => a.Id == id);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return mp;
        }

        public void InsertMealPlan(MealPlan mp, string userId, List<int> recipeIds) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    mp.FkUserId = context.Accounts.SingleOrDefault(a => a.Id == userId).Id;
                    mp.IsFullDay = false;
                    foreach(var id in recipeIds) {
                        var recipe = context.Recipes.FirstOrDefault(r => r.Id == id);
                        mp.Recipes.Add(recipe);
                    }
                    context.MealPlans.Add(mp);
                    context.SaveChanges();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    
        public void DeleteMealPlan(MealPlan mp) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    context.MealPlans.Remove(mp);
                    context.SaveChanges();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}
