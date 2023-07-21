using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CapstoneProject.Models;

namespace CapstoneProject.Views.UserRecipes
{
    public class CreateModel : PageModel
    {
        private readonly BusinessObjects.Models.RecipeOrganizerContext _context;

        public CreateModel(BusinessObjects.Models.RecipeOrganizerContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["FkRecipeId"] = new SelectList(_context.Recipes, "Id", "Id");
        ViewData["FkRecipeCategoryId"] = new SelectList(_context.RecipeCategories, "Id", "Name");
            return Page();
        }

        [BindProperty]
        public BusinessObjects.Models.Recipe Recipe { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Recipes.Add(Recipe);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
