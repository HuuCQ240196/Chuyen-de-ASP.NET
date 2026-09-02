using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProLap.Data;
using ProLap.Models;
using System.Data;
using System.Text.Json;

namespace ProLap.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ProLapDbContext _context;

        public CheckoutController(ProLapDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET: CHECKOUT
        // ==========================================
        [HttpGet]
        public IActionResult Index()
        {
            var cart = GetCart();

            if (cart.Count == 0)
            {
                return RedirectToAction(
                    "Index",
                    "Cart"
                );
            }

            ViewBag.Cart = cart;

            ViewBag.TotalAmount =
                cart.Sum(x => x.Total);

            return View(new Order());
        }

        // ==========================================
        // POST: CHECKOUT
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Order order)
        {
            var cart = GetCart();

            // ======================================
            // KIỂM TRA GIỎ HÀNG
            // ======================================
            if (cart.Count == 0)
            {
                return RedirectToAction(
                    "Index",
                    "Cart"
                );
            }

            // ======================================
            // KIỂM TRA DỮ LIỆU FORM
            // ======================================
            if (!ModelState.IsValid)
            {
                ViewBag.Cart = cart;

                ViewBag.TotalAmount =
                    cart.Sum(x => x.Total);

                return View(order);
            }

            // ======================================
            // BẮT ĐẦU TRANSACTION
            // ======================================
            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync(
                        IsolationLevel.Serializable
                    );

            try
            {
                // ======================================
                // DANH SÁCH SẢN PHẨM ĐÃ KIỂM TRA
                // ======================================
                var products =
                    new Dictionary<int, Product>();

                // ======================================
                // KIỂM TRA LẠI TỒN KHO
                // ======================================
                foreach (var cartItem in cart)
                {
                    var product =
                        await _context.Products
                            .FirstOrDefaultAsync(
                                p =>
                                    p.Id ==
                                    cartItem.ProductId
                            );

                    // Sản phẩm không còn tồn tại
                    if (product == null)
                    {
                        await transaction.RollbackAsync();

                        ModelState.AddModelError(
                            "",
                            $"Sản phẩm {cartItem.ProductName} không còn tồn tại."
                        );

                        ViewBag.Cart = cart;

                        ViewBag.TotalAmount =
                            cart.Sum(x => x.Total);

                        return View(order);
                    }

                    // ======================================
                    // SẢN PHẨM HẾT HÀNG
                    // ======================================
                    if (product.Stock <= 0)
                    {
                        await transaction.RollbackAsync();

                        ModelState.AddModelError(
                            "",
                            $"Sản phẩm {product.Name} hiện đã hết hàng."
                        );

                        ViewBag.Cart = cart;

                        ViewBag.TotalAmount =
                            cart.Sum(x => x.Total);

                        return View(order);
                    }

                    // ======================================
                    // KHÔNG ĐỦ TỒN KHO
                    // ======================================
                    if (product.Stock < cartItem.Quantity)
                    {
                        await transaction.RollbackAsync();

                        ModelState.AddModelError(
                            "",
                            $"Sản phẩm {product.Name} chỉ còn {product.Stock} sản phẩm trong kho."
                        );

                        ViewBag.Cart = cart;

                        ViewBag.TotalAmount =
                            cart.Sum(x => x.Total);

                        return View(order);
                    }

                    // Lưu sản phẩm đã kiểm tra
                    products[product.Id] = product;
                }

                // ======================================
                // TẠO ORDER
                // ======================================
                order.TotalAmount =
                    cart.Sum(x => x.Total);

                order.OrderDate =
                    DateTime.Now;

                order.Status =
                    "Chờ xác nhận";

                _context.Orders.Add(order);

                // ======================================
                // TẠO ORDER ITEM
                // VÀ TRỪ TỒN KHO
                // ======================================
                foreach (var cartItem in cart)
                {
                    var product =
                        products[cartItem.ProductId];

                    var orderItem =
                        new OrderItem
                        {
                            Order = order,

                            ProductId =
                                product.Id,

                            Quantity =
                                cartItem.Quantity,

                            Price =
                                cartItem.Price
                        };

                    _context.OrderItems
                        .Add(orderItem);

                    // ======================================
                    // TRỪ TỒN KHO
                    // ======================================
                    product.Stock -=
                        cartItem.Quantity;
                }

                // ======================================
                // LƯU TẤT CẢ THAY ĐỔI
                // ======================================
                await _context.SaveChangesAsync();

                // ======================================
                // COMMIT TRANSACTION
                // ======================================
                await transaction.CommitAsync();

                // ======================================
                // XÓA GIỎ HÀNG
                // ======================================
                HttpContext.Session.Remove("Cart");

                // ======================================
                // CHUYỂN SANG TRANG THÀNH CÔNG
                // ======================================
                return RedirectToAction(
                    nameof(Success),
                    new
                    {
                        id = order.Id
                    }
                );
            }
            catch
            {
                // ======================================
                // CÓ LỖI -> HỦY TOÀN BỘ
                // ======================================
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại."
                );

                ViewBag.Cart = cart;

                ViewBag.TotalAmount =
                    cart.Sum(x => x.Total);

                return View(order);
            }
        }

        // ==========================================
        // ĐẶT HÀNG THÀNH CÔNG
        // ==========================================
        public async Task<IActionResult> Success(int id)
        {
            var order =
                await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(
                        o => o.Id == id
                    );

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // ==========================================
        // LẤY GIỎ HÀNG TỪ SESSION
        // ==========================================
        private List<CartItem> GetCart()
        {
            string? cartJson =
                HttpContext.Session
                    .GetString("Cart");

            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }

            return JsonSerializer
                .Deserialize<List<CartItem>>(cartJson)
                ?? new List<CartItem>();
        }
    }
}