using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using SmartBreadcrumbs.Attributes;
using System.Collections.Generic;

namespace CapstoneProject.Controllers {
    [Authorize(Roles = "Admin")]
    public class RecipeCategoryController : Controller {

        private readonly IRecipeCategoryRepository _recipeCategoryRepository;

        public RecipeCategoryController(IRecipeCategoryRepository recipeCategoryRepository) {
            _recipeCategoryRepository = recipeCategoryRepository;
        }

        [Breadcrumb("Recipe Categories Management")]
        public IActionResult Index() {
            IEnumerable<RecipeCategory> objRecipeCategoryList = _recipeCategoryRepository.GetRecipeCategories();
            return View(objRecipeCategoryList);
        }


        //GET
        [Breadcrumb("Create", FromAction = "Index", FromController = typeof(RecipeCategoryController))]
        public IActionResult Create() {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RecipeCategory obj) {
            if (ModelState.IsValid) {
                _recipeCategoryRepository.InsertRecipeCategory(obj);
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET
        [Breadcrumb("Edit", FromAction = "Index", FromController = typeof(RecipeCategoryController))]
        public IActionResult Edit(int? id) {
            if (id == null || id == 0) {
                return NotFound();
            }
            var categoryFromDb = _recipeCategoryRepository.GetRecipeCategoryById(id);
            if (categoryFromDb == null) {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RecipeCategory obj) {
            if (ModelState.IsValid) {
                _recipeCategoryRepository.UpdateRecipeCategory(obj);
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET
        [Breadcrumb("Delete", FromAction = "Index", FromController = typeof(RecipeCategoryController))]
        public IActionResult Delete(int? id) {
            if (id == null || id == 0) {
                return NotFound();
            }
            var categoryFromDb = _recipeCategoryRepository.GetRecipeCategoryById(id);

            if (categoryFromDb == null) {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        //POST
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id) {
            var obj = _recipeCategoryRepository.GetRecipeCategoryById(id);
            if (obj == null) {
                return NotFound();
            }
            _recipeCategoryRepository.DeleteRecipeCategory(obj);
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
