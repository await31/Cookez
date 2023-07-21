using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessObjects {
    public class RecipeFeedbackDAO {
        private static RecipeFeedbackDAO instance = null;
        private static readonly object instacelock = new object();
        private RecipeFeedbackDAO() { }
        public static RecipeFeedbackDAO Instance {
            get {
                lock (instacelock) {
                    if (instance == null) {
                        instance = new RecipeFeedbackDAO();
                    }
                    return instance;
                }
            }
        }

        public List<RecipeFeedback> GetRecipeFeedbacks(int? recipeId) {
            List<RecipeFeedback> lists = new List<RecipeFeedback>();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    lists = context.RecipeFeedbacks
                        .OrderByDescending(x => x.CreatedDate)
                        .Where(a => a.RecipeId == recipeId)
                        .Include(a => a.User)
                        .ToList();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return lists;
        }

        public RecipeFeedback GetRecipeFeedbackById(int? id) {
            RecipeFeedback rf = new();
            try {
                using (var context = new RecipeOrganizerContext()) {
                    rf = context.RecipeFeedbacks
                        .FirstOrDefault(x => x.Id == id);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return rf;
        }

        public void InsertRecipeFeedback(RecipeFeedback rf) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                        context.RecipeFeedbacks.Add(rf);
                        context.SaveChanges();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteRecipeFeedback(RecipeFeedback rf) {
            try {
                using (var context = new RecipeOrganizerContext()) {
                    context.RecipeFeedbacks.Remove(rf);
                    context.SaveChanges();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}
