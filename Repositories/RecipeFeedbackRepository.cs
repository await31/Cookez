using BusinessObjects.Models;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public class RecipeFeedbackRepository : IRecipeFeedbackRepository {
        public IEnumerable<RecipeFeedback> GetRecipeFeedbacks(int? recipeId) => RecipeFeedbackDAO.Instance.GetRecipeFeedbacks(recipeId);
        public void InsertRecipeFeedback(RecipeFeedback rf) => RecipeFeedbackDAO.Instance.InsertRecipeFeedback(rf);
        public void DeleteRecipeFeedback(RecipeFeedback rf) => RecipeFeedbackDAO.Instance.DeleteRecipeFeedback(rf);
        public RecipeFeedback GetRecipeFeedbackById(int? id) => RecipeFeedbackDAO.Instance.GetRecipeFeedbackById(id);
    }
}
