using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
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

        [Authorize(Roles = StaticDetail.SupplyUser + "," + StaticDetail.ManagerUser)]
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

        [Authorize(Roles = StaticDetail.SupplyUser + "," + StaticDetail.ManagerUser)]
        public async Task<IActionResult> OrderPrepare(int OrderId)
        {
            OrderHeader orderHeader = await _applicationDbContext.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = StaticDetail.StatusInProcess;
            await _applicationDbContext.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = StaticDetail.SupplyUser + "," + StaticDetail.ManagerUser)]
        public async Task<IActionResult> OrderReady(int OrderId)
        {
            OrderHeader orderHeader = await _applicationDbContext.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = StaticDetail.StatusReady;
            await _applicationDbContext.SaveChangesAsync();

            //Email logic to notify user that order is ready for pickup

            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = StaticDetail.SupplyUser + "," + StaticDetail.ManagerUser)]
        public async Task<IActionResult> OrderCancel(int OrderId)
        {
            OrderHeader orderHeader = await _applicationDbContext.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = StaticDetail.StatusCancelled;
            await _applicationDbContext.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize]
        public async Task<IActionResult> OrderPickup(int productPage = 1, string searchName = null, string searchPhone = null, string searchEmail = null)
        {
            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            StringBuilder param = new StringBuilder();
            param.Append("/Customer/Order/OrderPickup?productPage=:");
            param.Append("&searchName=");
            if (searchName != null)
            {
                param.Append(searchName);
            }
            param.Append("&searchPhone=");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }
            param.Append("&searchEmail=");
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }

            List<OrderHeader> OrderHeaderList = new List<OrderHeader>();

            if (searchName != null || searchPhone != null || searchEmail != null)
            {
                var user = new ApplicationUser();

                if (searchName != null)
                {
                    OrderHeaderList = await _applicationDbContext.OrderHeader
                        .Include(a => a.ApplicationUser)
                        .Where(u => u.PickupName.ToLower().Contains(searchName.ToLower()))
                        .OrderByDescending(d => d.OrderDate).ToListAsync();
                }
                else
                {
                    if (searchEmail != null)
                    {
                        user = await _applicationDbContext.ApplicationUser
                            .Where(u => u.Email.ToLower().Contains(searchEmail.ToLower()))
                            .FirstOrDefaultAsync();
                        OrderHeaderList = await _applicationDbContext.OrderHeader
                            .Include(a => a.ApplicationUser)
                            .Where(o => o.UserId == user.Id)
                            .OrderByDescending(d => d.OrderDate).ToListAsync();
                    }
                    else
                    {
                        if (searchPhone != null)
                        {
                            OrderHeaderList = await _applicationDbContext.OrderHeader
                                .Include(a => a.ApplicationUser)
                                .Where(u => u.PhoneNumber.Contains(searchPhone))
                                .OrderByDescending(d => d.OrderDate).ToListAsync();
                        }
                    }
                }
            }
            else
            {
                OrderHeaderList = await _applicationDbContext.OrderHeader
                     .Include(a => a.ApplicationUser).Where(u => u.Status == StaticDetail.StatusReady).ToListAsync();
            }

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
                UrlParam = param.ToString()
            };

            return View(orderListVM);
        }

        [Authorize(Roles = StaticDetail.FrontDeskUser + "," + StaticDetail.ManagerUser)]
        [HttpPost]
        [ActionName("OrderPickup")]
        public async Task<IActionResult> OrderPickupPost(int orderId)
        {
            OrderHeader orderHeader = await _applicationDbContext.OrderHeader.FindAsync(orderId);
            orderHeader.Status = StaticDetail.StatusCompleted;
            await _applicationDbContext.SaveChangesAsync();
            return RedirectToAction("OrderPickup", "Order");
        }
    }
}