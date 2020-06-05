using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStore.UI.Data;
using WebStore.UI.Models.ViewModels;

namespace WebStore.UI.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public OrderController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _applicationDbContext.OrderHeader
                    .Include(o => o.ApplicationUser)
                    .FirstOrDefaultAsync(i => i.Id == id && i.UserId == claim.Value),
                OrderDetails = await _applicationDbContext.OrderDetails
                    .Where(o => o.OrderId == id).ToListAsync()
            };

            return View(orderDetailsViewModel);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}