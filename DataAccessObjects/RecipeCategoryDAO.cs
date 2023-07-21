using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessObjects {
    public class RecipeCategoryDAO {
        private static RecipeCategoryDAO instance = null;
        private static readonly object instacelock = new object();
        private RecipeCategoryDAO() { }
        public static RecipeCategoryDAO Instance {
            get {
                lock (instacelock) {
                    if (instance == null) {
                        instance = new RecipeCategoryDAO();
                    }
                    return instance;
                }
            }
        }

        public List<RecipeCategory> GetRecipeCategories() {
            List<RecipeCategory> listCategories = new List<RecipeCategory>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    listCategories = context.RecipeCategories.ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return listCategories;
        }

        public RecipeCategory GetRecipeCategoryById(int? id) {
            RecipeCategory rc = new();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    rc = context.RecipeCategories.SingleOrDefault(a => a.Id == id);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return rc;
        }

        public void InsertRecipeCategory(RecipeCategory rc) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetRecipeCategoryById(rc.Id);
                    if (cus == null) {
                        context.RecipeCategories.Add(rc);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Recipe Category is existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateRecipeCategory(RecipeCategory rc) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetRecipeCategoryById(rc.Id);
                    if (cus != null) {
                        context.RecipeCategories.Update(rc);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Recipe Category is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteRecipeCategory(RecipeCategory rc) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    var cus = GetRecipeCategoryById(rc.Id);
                    if (cus != null) {
                        context.RecipeCategories.Remove(rc);
                        context.SaveChanges();
                    } else {
                        throw new Exception("Recipe Category is not existed.");
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}
