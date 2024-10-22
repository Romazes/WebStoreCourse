﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebStore.UI.Data;
using WebStore.UI.Models;
using WebStore.UI.Utility;

namespace WebStore.UI.Areas.Admin.Controllers
{
    [Authorize(Roles = StaticDetail.ManagerUser)]
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CategoryController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        
        //GET - INDEX
        public async Task<IActionResult> Index()
        {
            return View(await _applicationDbContext.Category.ToListAsync());
        }

        //GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                //if valid
                _applicationDbContext.Category.Add(category);
                await _applicationDbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        //GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            var category = await _applicationDbContext.Category.FindAsync(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _applicationDbContext.Update(category);
                await _applicationDbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        //GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
            var category = await _applicationDbContext.Category.FindAsync(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
                return NotFound();
            var category = await _applicationDbContext.Category.FindAsync(id);
            if (category == null)
                return View();
            _applicationDbContext.Category.Remove(category);
            await _applicationDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET - DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var category = await _applicationDbContext.Category.FindAsync(id);
            if (category == null)
                return NotFound();
            return View(category);
        }
    }
}