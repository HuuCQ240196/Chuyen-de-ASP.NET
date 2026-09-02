using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProLap.Data;
using Prolap.Models;
using System.Diagnostics;

namespace Prolap.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProLapDbContext _context;

        public HomeController(ProLapDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // TRANG CHỦ
        // ==========================================
        public async Task<IActionResult> Index(
            string? searchString,
            int? brandId,
            int? categoryId,
            int page = 1)
        {
            // ======================================
            // SỐ SẢN PHẨM TRÊN MỖI TRANG
            // ======================================
            const int pageSize = 8;

            // Không cho số trang nhỏ hơn 1
            if (page < 1)
            {
                page = 1;
            }

            // ======================================
            // LẤY DANH SÁCH SẢN PHẨM
            // ======================================
            var products = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();

            // ======================================
            // TÌM KIẾM THEO TÊN SẢN PHẨM
            // ======================================
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchString));
            }

            // ======================================
            // LỌC THEO THƯƠNG HIỆU
            // ======================================
            if (brandId.HasValue)
            {
                products = products.Where(p =>
                    p.BrandId == brandId.Value);
            }

            // ======================================
            // LỌC THEO DANH MỤC
            // ======================================
            if (categoryId.HasValue)
            {
                products = products.Where(p =>
                    p.CategoryId == categoryId.Value);
            }

            // ======================================
            // TỔNG SỐ SẢN PHẨM SAU KHI LỌC
            // ======================================
            int totalProducts =
                await products.CountAsync();

            // ======================================
            // TÍNH TỔNG SỐ TRANG
            // ======================================
            int totalPages =
                (int)Math.Ceiling(
                    totalProducts / (double)pageSize
                );

            // ======================================
            // XỬ LÝ TRƯỜNG HỢP PAGE VƯỢT QUÁ
            // TỔNG SỐ TRANG
            // ======================================
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // ======================================
            // DROPDOWN THƯƠNG HIỆU
            // ======================================
            ViewBag.BrandId = new SelectList(
                await _context.Brands
                    .OrderBy(b => b.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                brandId
            );

            // ======================================
            // DROPDOWN DANH MỤC
            // ======================================
            ViewBag.CategoryId = new SelectList(
                await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                categoryId
            );

            // ======================================
            // GIỮ LẠI ĐIỀU KIỆN TÌM KIẾM
            // ======================================
            ViewBag.SearchString = searchString;

            ViewBag.SelectedBrandId = brandId;

            ViewBag.SelectedCategoryId = categoryId;

            // ======================================
            // THÔNG TIN PHÂN TRANG
            // ======================================
            ViewBag.CurrentPage = page;

            ViewBag.TotalPages = totalPages;

            ViewBag.TotalProducts = totalProducts;

            ViewBag.PageSize = pageSize;

            // ======================================
            // LẤY SẢN PHẨM CỦA TRANG HIỆN TẠI
            // ======================================
            var result = await products
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(result);
        }

        // ==========================================
        // TRANG PRIVACY
        // ==========================================
        public IActionResult Privacy()
        {
            return View();
        }

        // ==========================================
        // TRANG LỖI
        // ==========================================
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                }
            );
        }
    }
}