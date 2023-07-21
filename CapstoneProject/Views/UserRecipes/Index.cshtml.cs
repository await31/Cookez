using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Models;

namespace CapstoneProject.Views.UserRecipes
{
    public class IndexModel : PageModel
    {
        private readonly BusinessObjects.Models.RecipeOrganizerContext _context;

        public IndexModel(BusinessObjects.Models.RecipeOrganizerContext context)
        {
            _context = context;
        }

        public IList<BusinessObjects.Models.Recipe> Recipe { get;set; }

        public async Task OnGetAsync()
        {
            Recipe = await _context.Recipes
                .Include(r => r.FkRecipe)
                .Include(r => r.FkRecipeCategory).ToListAsync();
        }
    }
}
