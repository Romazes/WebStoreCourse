using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStore.UI.Data;
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