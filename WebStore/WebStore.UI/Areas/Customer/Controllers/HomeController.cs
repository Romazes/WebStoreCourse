using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebStore.UI.Data;
using WebStore.UI.Models;
using WebStore.UI.Models.ViewModels;

namespace WebStore.UI.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext applicationDbContext,ILogger<HomeController> logger)
        {
            _applicationDbContext = applicationDbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            IndexViewModel indexVM = new IndexViewModel()
            {
                MenuItem = await _applicationDbContext.MenuItem.Include(c => c.Category).Include(sc => sc.SubCategory).ToListAsync(),
                Category = await _applicationDbContext.Category.ToListAsync(),
                Coupon = await _applicationDbContext.Coupon.Where(ca => ca.IsActive == true).ToListAsync()
            };
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if(claim != null)
            {
                var cnt = _applicationDbContext.ShoppingCart.Where(u => u.ApplicationUserId == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32("startSessionCartCount", cnt);
            }

            return View(indexVM);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var menuItemFromDb = await _applicationDbContext.MenuItem.Include(c => c.Category)
                .Include(sc => sc.SubCategory).Where(i => i.Id == id).SingleOrDefaultAsync();

            ShoppingCart cartObj = new ShoppingCart()
            {
                MenuItem = menuItemFromDb,
                MenuItemId = menuItemFromDb.Id
            };

            return View(cartObj);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart cartObj)
        {
            cartObj.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cartObj.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = await _applicationDbContext.ShoppingCart
                    .Where(i => i.ApplicationUserId == cartObj.ApplicationUserId && i.MenuItemId == cartObj.MenuItemId)
                    .FirstOrDefaultAsync();

                if(cartFromDb == null)
                {
                    await _applicationDbContext.ShoppingCart.AddAsync(cartObj);
                }
                else
                {
                    cartFromDb.Count = cartFromDb.Count + cartObj.Count;
                }
                await _applicationDbContext.SaveChangesAsync();

                var count = _applicationDbContext.ShoppingCart.Where(c => c.ApplicationUserId == cartObj.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32("startSessionCartCount", count);

                return RedirectToAction("Index");
            }
            else
            {
                var menuItemFromDb = await _applicationDbContext.MenuItem.Include(c => c.Category)
                .Include(sc => sc.SubCategory).Where(i => i.Id == cartObj.MenuItemId).SingleOrDefaultAsync();

                ShoppingCart cartObject = new ShoppingCart()
                {
                    MenuItem = menuItemFromDb,
                    MenuItemId = menuItemFromDb.Id
                };

                return View(cartObject);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
