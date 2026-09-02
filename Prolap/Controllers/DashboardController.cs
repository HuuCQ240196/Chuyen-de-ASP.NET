using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProLap.Data;

namespace Prolap.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ProLapDbContext _context;

        public DashboardController(ProLapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ==========================================
            // TỔNG SỐ SẢN PHẨM
            // ==========================================
            ViewBag.TotalProducts =
                await _context.Products.CountAsync();

            // ==========================================
            // TỔNG SỐ ĐƠN HÀNG
            // ==========================================
            ViewBag.TotalOrders =
                await _context.Orders.CountAsync();

            // ==========================================
            // ĐƠN CHỜ XÁC NHẬN
            // ==========================================
            int pendingOrders =
                await _context.Orders.CountAsync(
                    o => o.Status == "Chờ xác nhận"
                );

            ViewBag.PendingOrders = pendingOrders;

            // ==========================================
            // ĐƠN ĐÃ XÁC NHẬN
            // ==========================================
            int confirmedOrders =
                await _context.Orders.CountAsync(
                    o => o.Status == "Đã xác nhận"
                );

            ViewBag.ConfirmedOrders = confirmedOrders;

            // ==========================================
            // ĐƠN ĐANG GIAO
            // ==========================================
            int shippingOrders =
                await _context.Orders.CountAsync(
                    o => o.Status == "Đang giao"
                );

            ViewBag.ShippingOrders = shippingOrders;

            // ==========================================
            // ĐƠN HOÀN THÀNH
            // ==========================================
            int completedOrders =
                await _context.Orders.CountAsync(
                    o => o.Status == "Hoàn thành"
                );

            ViewBag.CompletedOrders = completedOrders;

            // ==========================================
            // ĐƠN ĐÃ HỦY
            // ==========================================
            int cancelledOrders =
                await _context.Orders.CountAsync(
                    o => o.Status == "Đã hủy"
                );

            ViewBag.CancelledOrders = cancelledOrders;

            // ==========================================
            // DOANH THU TỔNG
            // Chỉ tính đơn hoàn thành
            // ==========================================
            ViewBag.TotalRevenue =
                await _context.Orders
                    .Where(o => o.Status == "Hoàn thành")
                    .SumAsync(o => (decimal?)o.TotalAmount)
                ?? 0;

            // ==========================================
            // SẢN PHẨM SẮP HẾT HÀNG
            // ==========================================
            ViewBag.LowStockProducts =
                await _context.Products
                    .CountAsync(p => p.Stock <= 5);

            // ==========================================
            // DỮ LIỆU BIỂU ĐỒ TRẠNG THÁI ĐƠN HÀNG
            // ==========================================
            ViewBag.ChartLabels = new[]
            {
                "Chờ xác nhận",
                "Đã xác nhận",
                "Đang giao",
                "Hoàn thành",
                "Đã hủy"
            };

            ViewBag.ChartData = new[]
            {
                pendingOrders,
                confirmedOrders,
                shippingOrders,
                completedOrders,
                cancelledOrders
            };


            // ==========================================
            // BIỂU ĐỒ DOANH THU THEO 12 THÁNG
            // ==========================================

            int currentYear = DateTime.Now.Year;

            // Lấy doanh thu của các đơn hoàn thành
            // trong năm hiện tại
            var monthlyRevenueData =
                await _context.Orders
                    .Where(o =>
                        o.Status == "Hoàn thành" &&
                        o.OrderDate.Year == currentYear)
                    .GroupBy(o => o.OrderDate.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        Revenue = g.Sum(o => o.TotalAmount)
                    })
                    .ToListAsync();


            // ==========================================
            // TẠO MẢNG 12 THÁNG
            // ==========================================

            decimal[] monthlyRevenue =
                new decimal[12];

            foreach (var item in monthlyRevenueData)
            {
                monthlyRevenue[item.Month - 1] =
                    item.Revenue;
            }


            // ==========================================
            // TÊN CÁC THÁNG
            // ==========================================

            ViewBag.RevenueChartLabels = new[]
            {
                "Tháng 1",
                "Tháng 2",
                "Tháng 3",
                "Tháng 4",
                "Tháng 5",
                "Tháng 6",
                "Tháng 7",
                "Tháng 8",
                "Tháng 9",
                "Tháng 10",
                "Tháng 11",
                "Tháng 12"
            };


            // ==========================================
            // DỮ LIỆU DOANH THU
            // ==========================================

            ViewBag.RevenueChartData =
                monthlyRevenue;


            // ==========================================
            // NĂM ĐANG THỐNG KÊ
            // ==========================================

            ViewBag.RevenueYear =
                currentYear;


            return View();
        }
    }
}