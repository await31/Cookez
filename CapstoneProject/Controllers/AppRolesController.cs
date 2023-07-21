using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SmartBreadcrumbs.Attributes;

namespace CapstoneProject.Controllers {

    [Authorize(Roles = "Admin")]
    public class AppRolesController : Controller {

        private readonly RoleManager<IdentityRole> _roleManager;

        public AppRolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        //List all the roles created by Users
        [Breadcrumb("Roles Management")]
        public IActionResult Index() {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        [Breadcrumb("Create", FromAction ="Index", FromController = typeof(AppRolesController))]
        public IActionResult Create() {
            return View();
        }

        [HttpPost]
        public IActionResult Create(IdentityRole model) {
            //Avoid duplicate role
            if (!_roleManager.RoleExistsAsync(model.Name).GetAwaiter().GetResult()) {
                _roleManager.CreateAsync(new IdentityRole(model.Name)).GetAwaiter().GetResult();
            }
            return RedirectToAction("Index");
        }
    }
}
