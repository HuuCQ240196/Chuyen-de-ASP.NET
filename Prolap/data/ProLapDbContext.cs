using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProLap.Models;

namespace ProLap.Data
{
    public class ProLapDbContext : IdentityDbContext<IdentityUser>
    {
        public ProLapDbContext(
            DbContextOptions<ProLapDbContext> options)
            : base(options)
        {
        }

        // ==========================================
        // THƯƠNG HIỆU
        // ==========================================
        public DbSet<Brand> Brands { get; set; }

        // ==========================================
        // DANH MỤC
        // ==========================================
        public DbSet<Category> Categories { get; set; }

        // ==========================================
        // SẢN PHẨM
        // ==========================================
        public DbSet<Product> Products { get; set; }

        // ==========================================
        // HÌNH ẢNH SẢN PHẨM
        // ==========================================
        public DbSet<ProductImage> ProductImages { get; set; }

        // ==========================================
        // ĐƠN HÀNG
        // ==========================================
        public DbSet<Order> Orders { get; set; }

        // ==========================================
        // CHI TIẾT ĐƠN HÀNG
        // ==========================================
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}