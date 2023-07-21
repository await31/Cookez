﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CapstoneProject.Models;

namespace CapstoneProject.Views.UserRecipes
{
    public class DetailsModel : PageModel
    {
        private readonly BusinessObjects.Models.RecipeOrganizerContext _context;

        public DetailsModel(BusinessObjects.Models.RecipeOrganizerContext context)
        {
            _context = context;
        }

        public BusinessObjects.Models.Recipe Recipe { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Recipe = await _context.Recipes
                .Include(r => r.FkRecipe)
                .Include(r => r.FkRecipeCategory).FirstOrDefaultAsync(m => m.Id == id);

            if (Recipe == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
