using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStore.UI.Data;
using WebStore.UI.Models;
using WebStore.UI.Utility;

namespace WebStore.UI.Areas.Admin.Controllers
{
    [Authorize(Roles = StaticDetail.ManagerUser)]
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

        //GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            Coupon = await _applicationDbContext.Coupon.SingleOrDefaultAsync(i => i.Id == id);

            if (Coupon == null)
                return NotFound();

            return View(Coupon);
        }

        //POST - EDIT
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfirmed(int? id)
        {
            if (id == null)
                return NotFound();

            var couponFromDb = await _applicationDbContext.Coupon.Where(i => i.Id == id).FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                //Work on the image
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
                    couponFromDb.Picture = picture1;
                }
                couponFromDb.Name = Coupon.Name;
                couponFromDb.CouponType = Coupon.CouponType;
                couponFromDb.Discount = Coupon.Discount;
                couponFromDb.MinimumAmount = Coupon.MinimumAmount;
                couponFromDb.IsActive = Coupon.IsActive;

                await _applicationDbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            Coupon.Picture = couponFromDb.Picture;
            return View(Coupon);
        }

        //GET - DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            Coupon = await _applicationDbContext.Coupon.SingleOrDefaultAsync(i => i.Id == id);

            if (Coupon == null)
                return NotFound();

            return View(Coupon);
        }

        //GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            Coupon = await _applicationDbContext.Coupon.SingleOrDefaultAsync(i => i.Id == id);

            if (Coupon == null)
                return NotFound();

            return View(Coupon);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
                return NotFound();
            Coupon coupon = await _applicationDbContext.Coupon.FindAsync(id);
            if (coupon == null)
                return NotFound();
            _applicationDbContext.Coupon.Remove(coupon);
            await _applicationDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}