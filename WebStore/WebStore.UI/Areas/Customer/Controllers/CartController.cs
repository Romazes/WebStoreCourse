using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStore.UI.Data;
using WebStore.UI.Models;
using WebStore.UI.Models.ViewModels;
using WebStore.UI.Utility;

namespace WebStore.UI.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        [BindProperty]
        public OrderDetailsCart DetailsCart { get; set; }

        public CartController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<IActionResult> Index()
        {
            DetailsCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader()
            };

            DetailsCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var cart = _applicationDbContext.ShoppingCart.Where(t => t.ApplicationUserId == claim.Value);
            if (cart != null)
                DetailsCart.ListCart = cart.ToList();

            foreach (var list in DetailsCart.ListCart)
            {
                list.MenuItem = await _applicationDbContext.MenuItem.FirstOrDefaultAsync(i => i.Id == list.MenuItemId);
                DetailsCart.OrderHeader.OrderTotal = DetailsCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);
                list.MenuItem.Description = StaticDetail.ConvertToRawHtml(list.MenuItem.Description);
                if (list.MenuItem.Description.Length > 100)
                {
                    list.MenuItem.Description = list.MenuItem.Description.Substring(0, 99) + "...";
                }
            }
            DetailsCart.OrderHeader.OrderTotalOriginal = DetailsCart.OrderHeader.OrderTotal;

            if (HttpContext.Session.GetString(StaticDetail.startSessionCouponCode) != null)
            {
                DetailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(StaticDetail.startSessionCouponCode);
                var couponFromDb = await _applicationDbContext.Coupon
                    .Where(t => t.Name.ToLower() == DetailsCart.OrderHeader.CouponCode.ToLower())
                    .FirstOrDefaultAsync();
                DetailsCart.OrderHeader.OrderTotal = StaticDetail
                    .DiscountedPrice(couponFromDb, DetailsCart.OrderHeader.OrderTotalOriginal);
            }

            return View(DetailsCart);
        }

        public async Task<IActionResult> Summary()
        {
            DetailsCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader()
            };

            DetailsCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser applicationUser = await _applicationDbContext.ApplicationUser.Where(i => i.Id == claim.Value).FirstOrDefaultAsync();

            var cart = _applicationDbContext.ShoppingCart.Where(t => t.ApplicationUserId == claim.Value);
            if (cart != null)
                DetailsCart.ListCart = cart.ToList();

            foreach (var list in DetailsCart.ListCart)
            {
                list.MenuItem = await _applicationDbContext.MenuItem.FirstOrDefaultAsync(i => i.Id == list.MenuItemId);
                DetailsCart.OrderHeader.OrderTotal = DetailsCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);
            }
            DetailsCart.OrderHeader.OrderTotalOriginal = DetailsCart.OrderHeader.OrderTotal;
            DetailsCart.OrderHeader.PickupName = applicationUser.Name;
            DetailsCart.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            DetailsCart.OrderHeader.PickUpTime = DateTime.Now;

            if (HttpContext.Session.GetString(StaticDetail.startSessionCouponCode) != null)
            {
                DetailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(StaticDetail.startSessionCouponCode);
                var couponFromDb = await _applicationDbContext.Coupon
                    .Where(t => t.Name.ToLower() == DetailsCart.OrderHeader.CouponCode.ToLower())
                    .FirstOrDefaultAsync();
                DetailsCart.OrderHeader.OrderTotal = StaticDetail
                    .DiscountedPrice(couponFromDb, DetailsCart.OrderHeader.OrderTotalOriginal);
            }

            return View(DetailsCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryConfirmed()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            DetailsCart.ListCart = await _applicationDbContext.ShoppingCart.Where(t => t.ApplicationUserId == claim.Value).ToListAsync();

            DetailsCart.OrderHeader.PaymentStatus = StaticDetail.PaymentStatusPending;
            DetailsCart.OrderHeader.OrderDate = DateTime.Now;
            DetailsCart.OrderHeader.UserId = claim.Value;
            DetailsCart.OrderHeader.Status = StaticDetail.PaymentStatusPending;
            DetailsCart.OrderHeader.PickUpTime = Convert.ToDateTime(DetailsCart.OrderHeader.PickUpDate.ToShortDateString() + " " + DetailsCart.OrderHeader.PickUpTime.ToShortTimeString());

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            _applicationDbContext.OrderHeader.Add(DetailsCart.OrderHeader);
            await _applicationDbContext.SaveChangesAsync();

            DetailsCart.OrderHeader.OrderTotalOriginal = 0;

            foreach (var item in DetailsCart.ListCart)
            {
                item.MenuItem = await _applicationDbContext.MenuItem.FirstOrDefaultAsync(i => i.Id == item.MenuItemId);
                OrderDetails orderDetails = new OrderDetails
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = DetailsCart.OrderHeader.Id,
                    Description = item.MenuItem.Description,
                    Name = item.MenuItem.Name,
                    Price = item.MenuItem.Price,
                    Count = item.Count
                };
                DetailsCart.OrderHeader.OrderTotalOriginal += orderDetails.Count * orderDetails.Price;
                _applicationDbContext.OrderDetails.Add(orderDetails);
            }

            if (HttpContext.Session.GetString(StaticDetail.startSessionCouponCode) != null)
            {
                DetailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(StaticDetail.startSessionCouponCode);
                var couponFromDb = await _applicationDbContext.Coupon
                    .Where(t => t.Name.ToLower() == DetailsCart.OrderHeader.CouponCode.ToLower())
                    .FirstOrDefaultAsync();
                DetailsCart.OrderHeader.OrderTotal = StaticDetail
                    .DiscountedPrice(couponFromDb, DetailsCart.OrderHeader.OrderTotalOriginal);
            }
            else
            {
                DetailsCart.OrderHeader.OrderTotal = DetailsCart.OrderHeader.OrderTotalOriginal;
            }
            DetailsCart.OrderHeader.CouponCodeDiscount = DetailsCart.OrderHeader.OrderTotalOriginal - DetailsCart.OrderHeader.OrderTotal;
            _applicationDbContext.ShoppingCart.RemoveRange(DetailsCart.ListCart);
            HttpContext.Session.SetInt32(StaticDetail.startSessionShoppingCartCount, 0);
            await _applicationDbContext.SaveChangesAsync();


            //return RedirectToAction("Confirm", "Order", new { id = DetailsCart.OrderHeader.Id });
            return RedirectToAction("Index", "Home");
        }


        public IActionResult AddCoupon()
        {
            if (DetailsCart.OrderHeader.CouponCode == null)
            {
                DetailsCart.OrderHeader.CouponCode = string.Empty;
            }

            HttpContext.Session.SetString(StaticDetail.startSessionCouponCode, DetailsCart.OrderHeader.CouponCode);

            return RedirectToAction(nameof(Index));
        }


        public IActionResult RemoveCoupon()
        {

            HttpContext.Session.SetString(StaticDetail.startSessionCouponCode, string.Empty);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _applicationDbContext.ShoppingCart.SingleOrDefaultAsync(i => i.Id == cartId);
            cart.Count += 1;
            await _applicationDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _applicationDbContext.ShoppingCart.SingleOrDefaultAsync(i => i.Id == cartId);
            if (cart.Count == 1)
            {
                _applicationDbContext.ShoppingCart.Remove(cart);
                await _applicationDbContext.SaveChangesAsync();

                var cnt = _applicationDbContext.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId)
                    .ToList().Count;
                HttpContext.Session.SetInt32(StaticDetail.startSessionShoppingCartCount, cnt);
            }
            else
            {
                cart.Count -= 1;
                await _applicationDbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _applicationDbContext.ShoppingCart.SingleOrDefaultAsync(i => i.Id == cartId);

            _applicationDbContext.ShoppingCart.Remove(cart);
            await _applicationDbContext.SaveChangesAsync();

            var cnt = _applicationDbContext.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId)
                .ToList().Count;
            HttpContext.Session.SetInt32(StaticDetail.startSessionShoppingCartCount, cnt);

            return RedirectToAction(nameof(Index));
        }
    }
}