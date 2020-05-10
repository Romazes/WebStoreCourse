using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebStore.UI.Data;

namespace WebStore.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MenuItemController(ApplicationDbContext applicationDbContext, IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var menuItems = await _applicationDbContext.MenuItem.Include(c => c.Category)
                .Include(sc => sc.SubCategory).ToListAsync();
            return View(menuItems);
        }
    }
}