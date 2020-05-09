using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebStore.UI.Data;
using WebStore.UI.Models;
using WebStore.UI.Models.ViewModels;

namespace WebStore.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        [TempData]
        public string StatusMessage { get; set; }

        public SubCategoryController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        //GET - INDEX
        public async Task<IActionResult> Index()
        {
            var subCategories = await _applicationDbContext.SubCategory
                                                           .Include(c => c.Category).ToListAsync();
            return View(subCategories);
        }

        //GET - CREATE
        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _applicationDbContext.Category.ToListAsync(),
                SubCategory = new SubCategory(),
                SubCategoryList = await _applicationDbContext.SubCategory.OrderBy(n => n.Name)
                    .Select(n => n.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _applicationDbContext.SubCategory.Include(c => c.Category)
                    .Where(n => n.Name == model.SubCategory.Name && n.Category.Id == model.SubCategory.CategoryId);

                if (doesSubCategoryExists.Count() > 0)
                {
                    //Error
                    StatusMessage = "Error : Sub Category exists under " + doesSubCategoryExists.First().Category.Name
                        + " category. Please use another name.";
                }
                else
                {
                    _applicationDbContext.SubCategory.Add(model.SubCategory);
                    await _applicationDbContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _applicationDbContext.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _applicationDbContext.SubCategory.OrderBy(n => n.Name)
                    .Select(n => n.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };

            return View(modelVM);
        }

        //GET - ALL SUBCATEGORY FOR CURRENT CATEGORY
        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();

            subCategories = await (from subCategory in _applicationDbContext.SubCategory
                                   where subCategory.CategoryId == id
                                   select subCategory).ToListAsync();
            return Json(new SelectList(subCategories, "Id", "Name"));
        }

        //GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var subCategory = await _applicationDbContext.SubCategory.SingleOrDefaultAsync(i => i.Id == id);

            if (subCategory == null)
                return NotFound();

            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _applicationDbContext.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _applicationDbContext.SubCategory.OrderBy(n => n.Name)
                    .Select(n => n.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _applicationDbContext.SubCategory.Include(c => c.Category)
                    .Where(n => n.Name == model.SubCategory.Name && n.Category.Id == model.SubCategory.CategoryId);

                if (doesSubCategoryExists.Count() > 0)
                {
                    //Error
                    StatusMessage = "Error : Sub Category exists under " + doesSubCategoryExists.First().Category.Name
                        + " category. Please use another name.";
                }
                else
                {
                    var subCategoryFromDb = await _applicationDbContext.SubCategory.FindAsync(id);
                    subCategoryFromDb.Name = model.SubCategory.Name;
                    await _applicationDbContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _applicationDbContext.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _applicationDbContext.SubCategory.OrderBy(n => n.Name)
                    .Select(n => n.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };

            return View(modelVM);
        }
    }
}