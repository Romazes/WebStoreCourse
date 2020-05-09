using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStore.UI.Data;
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
                SubCategory = new Models.SubCategory(),
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
    }
}