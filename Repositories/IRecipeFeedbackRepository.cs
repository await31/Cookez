using BusinessObjects.Models;
using DataAccessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories {
    public interface IRecipeFeedbackRepository {
        IEnumerable<RecipeFeedback> GetRecipeFeedbacks(int? recipeId);
        void InsertRecipeFeedback(RecipeFeedback rf);
        void DeleteRecipeFeedback(RecipeFeedback rf);
        RecipeFeedback GetRecipeFeedbackById(int? id);
    }
}
