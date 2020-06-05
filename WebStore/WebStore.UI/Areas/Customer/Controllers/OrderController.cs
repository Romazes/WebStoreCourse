using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStore.UI.Data;
using WebStore.UI.Models;
using WebStore.UI.Models.ViewModels;
using WebStore.UI.Utility;

namespace WebStore.UI.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private int PageSize = 2;

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


        public IActionResult GetOrderStatus(int Id)
        {
            return PartialView("_OrderStatus", _applicationDbContext.OrderHeader
                .Where(m => m.Id == Id).FirstOrDefault().Status);
        }

        [Authorize]
        public async Task<IActionResult> OrderHistory(int productPage = 1)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            List<OrderHeader> OrderHeaderList = await _applicationDbContext.OrderHeader
                .Include(a => a.ApplicationUser).Where(u => u.UserId == claim.Value).ToListAsync();

            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _applicationDbContext.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };
                orderListVM.Orders.Add(individual);
            }

            var amountOfOrders = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(p => p.OrderHeader.Id)
                .Skip((productPage - 1) * PageSize).Take(PageSize).ToList();

            orderListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = amountOfOrders,
                UrlParam = "/Customer/Order/OrderHistory?productPage=:"
            };

            return View(orderListVM);
        }

        [Authorize(Roles=StaticDetail.SupplyUser + "," + StaticDetail.ManagerUser)]
        public async Task<IActionResult> ManageOrder()
        {
            List<OrderDetailsViewModel> orderDetailsVM = new List<OrderDetailsViewModel>();
            

            List<OrderHeader> OrderHeaderList = await _applicationDbContext.OrderHeader
                .Where(s => s.Status == StaticDetail.StatusSubmitted || s.Status == StaticDetail.StatusInProcess)
                .OrderByDescending(o => o.PickUpTime).ToListAsync();

            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _applicationDbContext.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };
                orderDetailsVM.Add(individual);
            }
            return View(orderDetailsVM.OrderBy(o => o.OrderHeader.PickUpTime));
        }

        public async Task<IActionResult> GetOrderDetails(int Id)
        {
            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _applicationDbContext.OrderHeader.FirstOrDefaultAsync(i => i.Id == Id),
                OrderDetails = await _applicationDbContext.OrderDetails.Where(o => o.OrderId == Id).ToListAsync()
            };
            orderDetailsViewModel.OrderHeader.ApplicationUser
                = await _applicationDbContext.ApplicationUser
                .FirstOrDefaultAsync(i => i.Id == orderDetailsViewModel.OrderHeader.UserId);

            return PartialView("_IndividualOrderDetails", orderDetailsViewModel);
        }
    }
}