using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebStore.UI.Data;

namespace WebStore.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public UserController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            return View(await _applicationDbContext.ApplicationUser.Where(i => i.Id != claim.Value).ToListAsync());
        }

        public async Task<IActionResult> Lock(string id)
        {
            if (id == null)
                return NotFound();

            var applicationUser = await _applicationDbContext.ApplicationUser.FirstOrDefaultAsync(i => i.Id == id);

            if (applicationUser == null)
                return NotFound();

            applicationUser.LockoutEnd = DateTime.Now.AddYears(100);

            await _applicationDbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UnLock(string id)
        {
            if (id == null)
                return NotFound();

            var applicationUser = await _applicationDbContext.ApplicationUser.FirstOrDefaultAsync(i => i.Id == id);

            if (applicationUser == null)
                return NotFound();

            applicationUser.LockoutEnd = DateTime.Now;

            await _applicationDbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}