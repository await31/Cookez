using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using BusinessObjects.Models;
using Repositories;

namespace CapstoneProject.Controllers {

    public class ContactsController : Controller {
        private readonly IContactRepository _contactRepository;

        public ContactsController(IContactRepository contactRepository) {
            _contactRepository = contactRepository;
        }

        // GET: Contacts
        [Authorize(Roles = "Admin")]
        public IActionResult Index() {
            var contacts = _contactRepository.GetContacts();
            return View(contacts);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var contact = _contactRepository.GetContactById(id);
            if (contact == null) {
                return NotFound();
            }

            return View(contact);
        }

        public IActionResult Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,UserName,Email,Message,UserId")] Contact contact) {
            if (ModelState.IsValid) {
                _contactRepository.InsertContact(contact);
                return RedirectToAction(nameof(Index));
            }
            
            return View(contact);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax([Bind("Id,UserName,Email,Message,UserId")] Contact model) {
            if (ModelState.IsValid) {
                _contactRepository.InsertContact(model);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int? id) {
            if (id == null) {
                return NotFound();
            }
            var contact = _contactRepository.GetContactById(id);
            if (contact == null) {
                return NotFound();
            }
            return View(contact);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,UserName,Email,Message,UserId")] Contact contact) {
            if (id != contact.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                _contactRepository.UpdateContact(contact);
                return RedirectToAction(nameof(Index));
            }
            return View(contact);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int? id) {
            if (id == null) {
                return NotFound();
            }
            var contact = _contactRepository.GetContactById(id);
            if (contact == null) {
                return NotFound();
            }
            return View(contact);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id) {
            var contact = _contactRepository.GetContactById(id);
            if (contact != null) {
                _contactRepository.DeleteContact(contact);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        private bool ContactExists(int id) {
            return (_contactRepository.GetContacts()?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}