using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStore.UI.Data;

namespace WebStore.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

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
    }
}