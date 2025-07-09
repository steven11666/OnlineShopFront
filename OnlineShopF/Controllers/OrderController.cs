using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShopF.Data;
using OnlineShopF.Helpers;
using OnlineShopF.Models;
using OnlineShopF.Areas.Identity.Data; 


namespace OnlineShopF.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly OnlineShopContext _context;
        private readonly UserManager<OnlineShopUser> _userManager;

        public OrderController(OnlineShopContext context, UserManager<OnlineShopUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var orders = await _context.Order
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }


        public IActionResult Checkout() 
        {
            if (SessionHelper.GetObjectFromJson<List<OrderItem>>(HttpContext.Session, "cart") == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            var myOrder = new Order()
            {
                UserId = _userManager.GetUserId(User),
                UserName = _userManager.GetUserName(User),
                OrderItem = SessionHelper.GetObjectFromJson<List<OrderItem>>(HttpContext.Session, "cart")
            };
            myOrder.Total = myOrder.OrderItem.Sum(m => m.SubTotal);
            ViewBag.CartItems = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");
            return View(myOrder);
        }

        //將訂單新增到資料庫
        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)               
        {
            if (ModelState.IsValid)
            {
                var cart = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");
                if (cart == null || cart.Count == 0)
                {
                    return RedirectToAction("Index", "Cart");
                }

                order.OrderDate = DateTime.Now;
                order.IsPaid = false;
                order.Total = cart.Sum(c => c.SubTotal);
                order.UserId = _userManager.GetUserId(User);
                order.UserName = _userManager.GetUserName(User);
                order.OrderItem = new List<OrderItem>();

                foreach (var cartItem in cart)
                {
                    order.OrderItem.Add(new OrderItem
                    {
                        ProductId = cartItem.ProductId, 
                        Amount = cartItem.Amount,
                        SubTotal = cartItem.SubTotal
                    });
                }

                _context.Add(order);
                await _context.SaveChangesAsync();

                SessionHelper.Remove(HttpContext.Session, "cart");
                return RedirectToAction("ReviewOrder", new { Id = order.Id });
            }
            return View("Checkout", order);
        }

        public async Task<IActionResult> ReviewOrder(int? Id) 
        {
            if (Id == null)
            {
                return NotFound();
            }
            var order = await _context.Order.FirstOrDefaultAsync(m => m.Id == Id);
            if (order.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }
            else 
            {
                order.OrderItem = await _context.OrderItem.Where(a => a.Id == Id).ToListAsync();
                ViewBag.orderItems = GetOrderItems(order.Id);
            }
            return View(order);
        }
        private List<CartItem> GetOrderItems(int orderId)
        {

            var OrderItems = _context.OrderItem.Where(p => p.OrderId == orderId).ToList();

            List<CartItem> orderItems = new List<CartItem>();
            foreach (var orderitem in OrderItems)
            {
                CartItem item = new CartItem(orderitem);
                item.Product = _context.Product.Single(x => x.Id == orderitem.ProductId);
                orderItems.Add(item);
            }

            return orderItems;
        }
    }
}
