using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShopF.Data;
using OnlineShopF.Helpers;
using OnlineShopF.Models;

namespace OnlineShopF.Controllers
{
    public class CartController : Controller
    {
        private readonly OnlineShopContext _context;

        public CartController(OnlineShopContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<CartItem> CartItems = SessionHelper.
                GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");

            if (CartItems != null)
            {
                ViewBag.Total = CartItems.Sum(m => m.SubTotal);
            }
            else
            {
                ViewBag.Total = 0;
            }

            return View(CartItems);
        }
        public IActionResult AddtoCart(int id)
        {
            var product = _context.Product.Single(x => x.Id == id);

            CartItem item = new CartItem
            {
                ProductId = product.Id, 
                Product = _context.Product.Single(x => x.Id.Equals(id)),
                Amount = 1,
                SubTotal = _context.Product.Single(m => m.Id == id).Price
            };

            if (SessionHelper.
                GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart") == null)
            {
                List<CartItem> cart = new List<CartItem>();
                cart.Add(item);
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            }
            else
            {
                List<CartItem> cart = SessionHelper.
                    GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");

                int index = cart.FindIndex(m => m.Product.Id.Equals(id));
                if (index != -1)
                {
                    cart[index].Amount += item.Amount;
                    cart[index].SubTotal += item.SubTotal;
                }
                else
                {
                    cart.Add(item);
                }
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            }

            return NoContent(); 
        }

        public IActionResult RemoveItem(int id)
        {
            List<CartItem> cart = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");
            int index = cart.FindIndex(m => m.Product.Id.Equals(id)); 
            cart.RemoveAt(index);

            if (cart.Count < 1)
            {
                SessionHelper.Remove(HttpContext.Session, "cart");
            }
            else
            {
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            }

            return RedirectToAction("Index");
        }
    }
}
