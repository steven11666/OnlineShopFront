using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OnlineShopF.Models;

namespace OnlineShopF.Data
{
    public class OnlineShopContext : DbContext
    {
        public OnlineShopContext (DbContextOptions<OnlineShopContext> options)
            : base(options)
        {
        }

        public DbSet<OnlineShopF.Models.Product> Product { get; set; } = default!;
        public DbSet<OnlineShopF.Models.Category> Category { get; set; }
        public DbSet<OnlineShopF.Models.Order> Order { get; set; }
        public DbSet<OnlineShopF.Models.OrderItem> OrderItem { get; set; }
    }
}
