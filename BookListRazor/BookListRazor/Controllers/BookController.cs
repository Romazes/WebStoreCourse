using BookListRazor.Model;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
        public IActionResult GetAll()
        {
            return Json(new { data = _appDbContext.Book.ToList() });
        }
    }
}