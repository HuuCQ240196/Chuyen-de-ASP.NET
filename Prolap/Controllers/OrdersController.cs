
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProLap.Data;

namespace Prolap.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly ProLapDbContext _context;

        public OrdersController(ProLapDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // DANH SÁCH ĐƠN HÀNG + LỌC TRẠNG THÁI
        // ==========================================
        public async Task<IActionResult> Index(string? status)
        {
            var orders = _context.Orders
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                orders = orders.Where(o =>
                    o.Status == status);
            }

            ViewBag.SelectedStatus = status;

            var result = await orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(result);
        }

        // ==========================================
        // CHI TIẾT ĐƠN HÀNG
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // ==========================================
        // CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Các trạng thái hợp lệ
            var validStatuses = new[]
            {
                "Chờ xác nhận",
                "Đã xác nhận",
                "Đang giao",
                "Hoàn thành",
                "Đã hủy"
            };

            if (!validStatuses.Contains(status))
            {
                return BadRequest();
            }

            // ==========================================
            // KHÔNG CHO THAY ĐỔI ĐƠN ĐÃ HỦY
            // ==========================================
            if (order.Status == "Đã hủy")
            {
                TempData["ErrorMessage"] =
                    "Đơn hàng đã hủy nên không thể thay đổi trạng thái.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = order.Id }
                );
            }

            // ==========================================
            // KHÔNG CHO THAY ĐỔI ĐƠN ĐÃ HOÀN THÀNH
            // ==========================================
            if (order.Status == "Hoàn thành")
            {
                TempData["ErrorMessage"] =
                    "Đơn hàng đã hoàn thành nên không thể thay đổi trạng thái.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = order.Id }
                );
            }

            // ==========================================
            // NẾU CHUYỂN SANG ĐÃ HỦY
            // THÌ HOÀN LẠI TỒN KHO
            // ==========================================
            if (status == "Đã hủy")
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.Stock += item.Quantity;
                    }
                }
            }

            order.Status = status;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Cập nhật trạng thái đơn hàng thành công.";

            return RedirectToAction(
                nameof(Details),
                new { id = order.Id }
            );
        }
    }
}