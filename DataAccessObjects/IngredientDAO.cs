using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessObjects {
    public class IngredientDAO {
        private static IngredientDAO instance = null;
        private static readonly object instacelock = new object();
        private IngredientDAO() { }
        public static IngredientDAO Instance {
            get {
                lock (instacelock) {
                    if (instance == null) {
                        instance = new IngredientDAO();
                    }
                    return instance;
                }
            }
        }

        public List<Ingredient> GetIngredients() {
            List<Ingredient> lists = new List<Ingredient>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Ingredients
                        .Include(a=>a.FkCategory)
                        .Include(a => a.IngredientNutrition)
                        .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public List<Ingredient> GetStatusTrueIngredients() {
            List<Ingredient> lists = new List<Ingredient>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Ingredients
                        .Where(a=>a.Status == true)
                        .Include(a=>a.IngredientNutrition)
                        .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public List<Ingredient> GetStatusFalseIngredients() {
            List<Ingredient> lists = new List<Ingredient>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.Ingredients
                        .Where(a => a.Status == false)
                        .Include(a => a.IngredientNutrition)
                        .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public Ingredient GetIngredientById(int? id) {
            Ingredient i = new();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    i = context.Ingredients
                        .Include(a=>a.FkCategory)
                        .Include(a => a.IngredientNutrition)
                        .SingleOrDefault(a => a.Id == id);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return i;
        }

        public void InsertIngredient(Ingredient i) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetIngredientById(i.Id);
                    if (cus == null) {
                        context.Ingredients.Add(i);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Ingredient is existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateIngredient(Ingredient i) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetIngredientById(i.Id);
                    if (cus != null) {
                        context.Ingredients.Update(i);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Ingredient is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteIngredient(Ingredient i) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetIngredientById(i.Id);
                    if (cus != null) {
                        var ingredientListInRecipe = context.RecipeIngredient.Where(ri => ri.IngredientId == i.Id);
                        context.RemoveRange(ingredientListInRecipe);
                        context.Ingredients.Remove(i);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Ingredient is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void Deny(Ingredient i) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetIngredientById(i.Id);
                    if (cus != null) {
                        context.Ingredients.Remove(i);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Ingredient is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void Approve(Ingredient i) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var ingredient = context.Ingredients.FirstOrDefault(ingredient => ingredient.Id == i.Id);
                    if (ingredient != null) {
                        ingredient.Status = true; // Set the status to approved
                        context.SaveChanges();
                    } else {
                        throw new Exception("Ingredient is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}
