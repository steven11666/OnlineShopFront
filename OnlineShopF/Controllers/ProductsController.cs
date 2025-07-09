using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineShopF.Data;
using OnlineShopF.Models;

namespace OnlineShopF.Controllers
{
    public class ProductsController : Controller
    {
        private readonly OnlineShopContext _context;

        public ProductsController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(int? cId, string? searchString)
        {
            var query = _context.Product.Include(p => p.Category).AsQueryable();

            if (cId != null)
            {
                query = query.Where(p => p.CategoryId == cId);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString));
            }

            var products = await query.ToListAsync();

            var dvm = products.Select(p => new DetailViewModel
            {
                product = p,
                imgsrc = ViewImage(p.Image)
            }).ToList();

            ViewBag.count = dvm.Count;

            return View(dvm);
        }



        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Product.FindAsync(id);
            if (product == null)
                return NotFound();

            string imgSrc = null;
            if (product.Image != null && product.Image.Length > 0)
            {
                var base64 = Convert.ToBase64String(product.Image);
                imgSrc = $"data:image/png;base64,{base64}";
            }

            var viewModel = new DetailViewModel
            {
                product = product,
                imgsrc = imgSrc
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int Id, string myComment)
        {
            var comment = new Comment()
            {
                ProductID = Id,
                Content = myComment,
                UserName = HttpContext.User.Identity.Name,
                Time = DateTime.Now
            };
            _context.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = Id });
        }


        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }
        private string ViewImage(byte[] arrayImage)
        {
            string base64String = Convert.ToBase64String(arrayImage, 0, arrayImage.Length);
            return "data:image/png;base64," + base64String;
        }
    }
}
