using BookListRazor.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookListRazor.Controllers
{
    [Route("api/Book")]
    [ApiController]
    public class BookController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public BookController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Json(new { data = await _appDbContext.Book.ToListAsync() });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var bookFromDb = await _appDbContext.Book.FirstOrDefaultAsync(t => t.Id == id);
            if (bookFromDb == null)
                return Json(new { success = false, message = "Error while Deletung" });
            _appDbContext.Book.Remove(bookFromDb);
            await _appDbContext.SaveChangesAsync();
            return Json(new { success = true, message = "Delete successful" });
        }
    }
}