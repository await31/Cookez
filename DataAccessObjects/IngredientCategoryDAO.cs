using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects {
    public class IngredientCategoryDAO {
        private static IngredientCategoryDAO instance = null;
        private static readonly object instacelock = new object();
        private IngredientCategoryDAO() { }
        public static IngredientCategoryDAO Instance {
            get {
                lock (instacelock) {
                    if (instance == null) {
                        instance = new IngredientCategoryDAO();
                    }
                    return instance;
                }
            }
        }

         public List<IngredientCategory> GetIngredientCategories() {
            List<IngredientCategory> listCategories = new List<IngredientCategory>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    listCategories = context.IngredientCategories.ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return listCategories;
        }

        public IngredientCategory GetIngredientCategoryById(int? id) {
            IngredientCategory ic = new();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    ic = context.IngredientCategories.SingleOrDefault(a => a.Id == id);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return ic;
        }

        public void InsertIngredientCategory(IngredientCategory ic) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetIngredientCategoryById(ic.Id);
                    if (cus == null) {
                        context.IngredientCategories.Add(ic);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Ingredient Category is existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateIngredientCategory(IngredientCategory ic) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetIngredientCategoryById(ic.Id);
                    if (cus != null) {
                        context.IngredientCategories.Update(ic);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Ingredient Category is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteIngredientCategory(IngredientCategory ic) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetIngredientCategoryById(ic.Id);
                    if (cus != null) {
                        context.IngredientCategories.Remove(ic);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Ingredient Category is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}
