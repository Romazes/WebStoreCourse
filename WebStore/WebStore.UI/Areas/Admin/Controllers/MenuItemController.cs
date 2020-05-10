using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebStore.UI.Data;
using WebStore.UI.Models.ViewModels;

namespace WebStore.UI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        public MenuItemController(ApplicationDbContext applicationDbContext, IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _webHostEnvironment = webHostEnvironment;
            MenuItemVM = new MenuItemViewModel()
            {
                Category = _applicationDbContext.Category,
                MenuItem = new Models.MenuItem()
            };
        }

        public async Task<IActionResult> Index()
        {
            var menuItems = await _applicationDbContext.MenuItem.Include(c => c.Category)
                .Include(sc => sc.SubCategory).ToListAsync();
            return View(menuItems);
        }

        //GET - CREATE
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        //POST - CREATE
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConfirmed()
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            //MenuItemVM.MenuItem.SubCategoryId = int.TryParse(Request.Form["SubCategoryId"].ToString(), MenuItemVM.MenuItem.SubCategoryId);

            if (!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            _applicationDbContext.MenuItem.Add(MenuItemVM.MenuItem);
            await _applicationDbContext.SaveChangesAsync();

            //Work on the image saving section
            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = await _applicationDbContext.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Count > 0)
            {
                //Files has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                menuItemFromDb.PictureUri = @"\images\" + MenuItemVM.MenuItem.Id + extension;
            }
            else
            {
                //No file was uploaded, so use default.
                var uploads = Path.Combine(webRootPath, @"images\" + Utility.StaticDetail.DefaultTeaPictureUri);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png");
                menuItemFromDb.PictureUri = @"\images\" + MenuItemVM.MenuItem.Id + ".png";
            }

            await _applicationDbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            MenuItemVM.MenuItem = await _applicationDbContext.MenuItem.Include(c => c.Category)
                .Include(sc => sc.SubCategory).SingleOrDefaultAsync(i => i.Id == id);
            MenuItemVM.SubCategory = await _applicationDbContext.SubCategory.Where(c => c.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();

            if (MenuItemVM.MenuItem == null)
                return NotFound();

            return View(MenuItemVM);
        }

        //POST - EDIT
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfirmed(int? id)
        {
            if (id == null)
                return NotFound();

            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            _applicationDbContext.MenuItem.Add(MenuItemVM.MenuItem);
            await _applicationDbContext.SaveChangesAsync();

            //Work on the image saving section
            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = await _applicationDbContext.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Count > 0)
            {
                //Files has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                menuItemFromDb.PictureUri = @"\images\" + MenuItemVM.MenuItem.Id + extension;
            }
            else
            {
                //No file was uploaded, so use default.
                var uploads = Path.Combine(webRootPath, @"images\" + Utility.StaticDetail.DefaultTeaPictureUri);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png");
                menuItemFromDb.PictureUri = @"\images\" + MenuItemVM.MenuItem.Id + ".png";
            }

            await _applicationDbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}