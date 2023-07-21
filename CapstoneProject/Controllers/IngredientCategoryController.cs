using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using SmartBreadcrumbs.Attributes;
using System.Collections.Generic;

namespace CapstoneProject.Controllers {
    [Authorize(Roles = "Admin")]
    public class IngredientCategoryController : Controller {

        private readonly IIngredientCategoryRepository _ingredientCategoryRepository;

        public IngredientCategoryController(IIngredientCategoryRepository ingredientCategoryRepository) {
            _ingredientCategoryRepository = ingredientCategoryRepository;
        }

        [Breadcrumb("Ingredient Categories Management")]
        public IActionResult Index() {
            IEnumerable<IngredientCategory> objIngredientCategoryList = _ingredientCategoryRepository.GetIngredientCategories();
            return View(objIngredientCategoryList);
        }

        // GET
        [Breadcrumb("Create", FromAction = "Index", FromController = typeof(IngredientCategoryController))]
        public IActionResult Create() {
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(IngredientCategory obj) {
            if (ModelState.IsValid) {
                _ingredientCategoryRepository.InsertIngredientCategory(obj);
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        // GET
        [Breadcrumb("Edit", FromAction = "Index", FromController = typeof(IngredientCategoryController))]
        public IActionResult Edit(int? id) {
            if (id == null || id == 0) {
                return NotFound();
            }
            var categoryFromDb = _ingredientCategoryRepository.GetIngredientCategoryById(id);
            if (categoryFromDb == null) {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(IngredientCategory obj) {
            if (ModelState.IsValid) {
                _ingredientCategoryRepository.UpdateIngredientCategory(obj);
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        // GET
        [Breadcrumb("Delete", FromAction = "Index", FromController = typeof(IngredientCategoryController))]
        public IActionResult Delete(int? id) {
            if (id == null || id == 0) {
                return NotFound();
            }
            var categoryFromDb = _ingredientCategoryRepository.GetIngredientCategoryById(id);
            if (categoryFromDb == null) {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        // POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id) {
            var obj = _ingredientCategoryRepository.GetIngredientCategoryById(id);
            if (obj == null) {
                return NotFound();
            }
            _ingredientCategoryRepository.DeleteIngredientCategory(obj);
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
