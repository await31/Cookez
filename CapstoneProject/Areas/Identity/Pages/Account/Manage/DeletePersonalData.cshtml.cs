using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CapstoneProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CapstoneProject.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<BusinessObjects.Models.Account> _userManager;
        private readonly SignInManager<BusinessObjects.Models.Account> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly BusinessObjects.Models.RecipeOrganizerContext _context;

        public DeletePersonalDataModel(
            UserManager<BusinessObjects.Models.Account> userManager,
            SignInManager<BusinessObjects.Models.Account> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            BusinessObjects.Models.RecipeOrganizerContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }
            var currUserId = await _userManager.GetUserIdAsync(user);
            var recipeList = _context.Recipes.Include(f => f.FkUser).Where(f => f.FkUser.Id.Equals(currUserId)).ToList();
            foreach(var recipe in recipeList)
            {
                recipe.FkUser = null;
                _context.Update(recipe);
                await _context.SaveChangesAsync();
            }
            _context.Favourites.RemoveRange(_context.Favourites.Include(f => f.Account).Where(f=>f.Account.Id.Equals(currUserId)));
            _context.MealPlans.RemoveRange(_context.MealPlans.Include(f => f.FkUser).Where(f=>f.FkUser.Id.Equals(currUserId)));
            var a = _context.RecipeFeedbacks.Include(f => f.User).Where(f => f.User.Id.Equals(currUserId)).ToList();
            foreach (var r in a)
            {
                _context.RecipeFeedbacks.Remove(r);
            }

            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{currUserId}'.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", currUserId);

            return Redirect("~/");
        }
    }
}
