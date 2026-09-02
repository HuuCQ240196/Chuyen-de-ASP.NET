using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProLap.Data;
using ProLap.Models;
using System.Text.Json;

namespace ProLap.Controllers
{
    public class CartController : Controller
    {
        private readonly ProLapDbContext _context;

        public CartController(ProLapDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // XEM GIỎ HÀNG
        // ==========================================
        public IActionResult Index()
        {
            var cart = GetCart();

            return View(cart);
        }

        // ==========================================
        // THÊM SẢN PHẨM VÀO GIỎ
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(
            int productId,
            int quantity = 1)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            // ==========================================
            // KIỂM TRA TỒN KHO
            // ==========================================
            if (product.Stock <= 0)
            {
                TempData["CartError"] =
                    "Sản phẩm hiện đã hết hàng.";

                return RedirectToAction(
                    "Details",
                    "Products",
                    new { id = productId }
                );
            }

            // Không cho số lượng nhỏ hơn 1
            if (quantity < 1)
            {
                quantity = 1;
            }

            var cart = GetCart();

            var existingItem = cart
                .FirstOrDefault(x =>
                    x.ProductId == productId);

            // ==========================================
            // SẢN PHẨM ĐÃ CÓ TRONG GIỎ
            // ==========================================
            if (existingItem != null)
            {
                int newQuantity =
                    existingItem.Quantity + quantity;

                // Không cho vượt tồn kho
                if (newQuantity > product.Stock)
                {
                    TempData["CartError"] =
                        $"Chỉ còn {product.Stock} sản phẩm trong kho.";

                    return RedirectToAction(
                        "Details",
                        "Products",
                        new { id = productId }
                    );
                }

                existingItem.Quantity =
                    newQuantity;
            }

            // ==========================================
            // SẢN PHẨM CHƯA CÓ TRONG GIỎ
            // ==========================================
            else
            {
                // Số lượng yêu cầu vượt tồn kho
                if (quantity > product.Stock)
                {
                    TempData["CartError"] =
                        $"Chỉ còn {product.Stock} sản phẩm trong kho.";

                    return RedirectToAction(
                        "Details",
                        "Products",
                        new { id = productId }
                    );
                }

                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            SaveCart(cart);

            TempData["CartMessage"] =
                "Đã thêm sản phẩm vào giỏ hàng.";

            return RedirectToAction(
                "Details",
                "Products",
                new { id = productId }
            );
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

        // ==========================================
        // LƯU GIỎ HÀNG VÀO SESSION
        // ==========================================
        private void SaveCart(
            List<CartItem> cart)
        {
            string cartJson =
                JsonSerializer.Serialize(cart);

            HttpContext.Session
                .SetString("Cart", cartJson);
        }

        // ==========================================
        // TĂNG SỐ LƯỢNG
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Increase(
            int productId)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x =>
                x.ProductId == productId);

            if (item == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Lấy tồn kho thực tế từ database
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == productId);

            if (product == null)
            {
                TempData["CartError"] =
                    "Sản phẩm không còn tồn tại.";

                return RedirectToAction(nameof(Index));
            }

            // ==========================================
            // KHÔNG CHO VƯỢT TỒN KHO
            // ==========================================
            if (item.Quantity >= product.Stock)
            {
                TempData["CartError"] =
                    $"Sản phẩm chỉ còn {product.Stock} sản phẩm trong kho.";

                return RedirectToAction(nameof(Index));
            }

            item.Quantity++;

            SaveCart(cart);

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // GIẢM SỐ LƯỢNG
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Decrease(
            int productId)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x =>
                x.ProductId == productId);

            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    cart.Remove(item);
                }

                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // XÓA SẢN PHẨM KHỎI GIỎ
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(
            int productId)
        {
            var cart = GetCart();

            var item = cart.FirstOrDefault(x =>
                x.ProductId == productId);

            if (item != null)
            {
                cart.Remove(item);

                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}