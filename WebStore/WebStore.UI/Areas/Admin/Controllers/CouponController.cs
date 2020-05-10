using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStore.UI.Data;
using WebStore.UI.Models;

namespace WebStore.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        [BindProperty]
        public Coupon Coupon { get; set; }

        public CouponController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
            Coupon = new Coupon();
        }

        public async Task<IActionResult> Index()
        {
            return View(await _applicationDbContext.Coupon.ToListAsync());
        }

        //GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        //POST - CREATE
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConfirmed()
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    byte[] picture1 = null;
                    using (var fileStream1 = files[0].OpenReadStream())
                    {
                        using (var memoryStream1 = new MemoryStream())
                        {
                            fileStream1.CopyTo(memoryStream1);
                            picture1 = memoryStream1.ToArray();
                        }
                    }
                    Coupon.Picture = picture1;
                }
                _applicationDbContext.Coupon.Add(Coupon);
                await _applicationDbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(Coupon);
        }
    }
}