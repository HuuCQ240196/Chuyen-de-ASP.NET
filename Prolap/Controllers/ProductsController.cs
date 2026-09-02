using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProLap.Data;
using ProLap.Models;

[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly ProLapDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductsController(
        ProLapDbContext context,
        IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // =====================================================
    // INDEX
    // =====================================================

    // GET: Products
    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .ToListAsync();

        return View(products);
    }

    // =====================================================
    // DETAILS
    // =====================================================

    // GET: Products/Details/5
    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // =====================================================
    // CREATE
    // =====================================================

    // GET: Products/Create
    public IActionResult Create()
    {
        LoadDropdowns();

        return View();
    }

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Name,Price,CPU,RAM,Storage,GPU,Screen,Stock,Description,BrandId,CategoryId")]
        Product product,
        List<IFormFile>? ImageFiles,
        IFormFile? ImageFile)
    {
        if (ModelState.IsValid)
        {
            // Lưu Product trước để lấy ProductId
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var uploadedImages = new List<string>();

            // =================================================
            // Trường hợp mới: upload nhiều ảnh
            // =================================================
            if (ImageFiles != null && ImageFiles.Count > 0)
            {
                foreach (var image in ImageFiles)
                {
                    if (image != null && image.Length > 0)
                    {
                        string imageUrl = await SaveImage(image);

                        uploadedImages.Add(imageUrl);

                        var productImage = new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = imageUrl
                        };

                        _context.ProductImages.Add(productImage);
                    }
                }
            }

            // =================================================
            // Tương thích form cũ: upload 1 ảnh
            // =================================================
            else if (ImageFile != null && ImageFile.Length > 0)
            {
                string imageUrl = await SaveImage(ImageFile);

                uploadedImages.Add(imageUrl);

                var productImage = new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = imageUrl
                };

                _context.ProductImages.Add(productImage);
            }

            // =================================================
            // Ảnh đầu tiên sẽ được dùng làm ảnh đại diện
            // =================================================
            if (uploadedImages.Count > 0)
            {
                product.ImageUrl = uploadedImages[0];

                _context.Products.Update(product);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        LoadDropdowns(
            product.BrandId,
            product.CategoryId
        );

        return View(product);
    }

    // =====================================================
    // EDIT
    // =====================================================

    // GET: Products/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        LoadDropdowns(
            product.BrandId,
            product.CategoryId
        );

        return View(product);
    }

    // POST: Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,Name,Price,CPU,RAM,Storage,GPU,Screen,Stock,Description,BrandId,CategoryId")]
        Product product,
        IFormFile? ImageFile,
        List<IFormFile>? ImageFiles)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        var existingProduct = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existingProduct == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Giữ ảnh đại diện cũ
                product.ImageUrl =
                    existingProduct.ImageUrl ?? string.Empty;

                // =================================================
                // Nếu Edit gửi nhiều ảnh mới
                // -> thêm vào bộ sưu tập ảnh
                // =================================================
                if (ImageFiles != null && ImageFiles.Count > 0)
                {
                    foreach (var image in ImageFiles)
                    {
                        if (image != null && image.Length > 0)
                        {
                            string imageUrl =
                                await SaveImage(image);

                            var productImage =
                                new ProductImage
                                {
                                    ProductId = product.Id,
                                    ImageUrl = imageUrl
                                };

                            _context.ProductImages.Add(
                                productImage
                            );

                            // Nếu sản phẩm chưa có ảnh đại diện
                            // thì lấy ảnh đầu tiên làm ảnh đại diện
                            if (string.IsNullOrEmpty(
                                product.ImageUrl))
                            {
                                product.ImageUrl = imageUrl;
                            }
                        }
                    }
                }

                // =================================================
                // Tương thích Edit cũ:
                // chọn 1 ảnh để thay ảnh đại diện
                // =================================================
                else if (ImageFile != null &&
                         ImageFile.Length > 0)
                {
                    string newImageUrl =
                        await SaveImage(ImageFile);

                    // Thêm ảnh mới vào ProductImages
                    var productImage =
                        new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = newImageUrl
                        };

                    _context.ProductImages.Add(
                        productImage
                    );

                    // Đổi ảnh đại diện
                    product.ImageUrl =
                        newImageUrl;
                }

                _context.Products.Update(product);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        product.ImageUrl =
            existingProduct.ImageUrl ?? string.Empty;

        LoadDropdowns(
            product.BrandId,
            product.CategoryId
        );

        return View(product);
    }

    // =====================================================
    // DELETE
    // =====================================================

    // GET: Products/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return RedirectToAction(nameof(Index));
        }

        // Lưu danh sách đường dẫn ảnh trước khi xóa DB
        var imageUrls = product.ProductImages
            .Select(i => i.ImageUrl)
            .ToList();

        // Nếu ImageUrl đại diện là ảnh cũ chưa nằm trong
        // ProductImages thì cũng thêm vào danh sách xóa
        if (!string.IsNullOrWhiteSpace(product.ImageUrl) &&
            !imageUrls.Contains(product.ImageUrl))
        {
            imageUrls.Add(product.ImageUrl);
        }

        // Xóa các bản ghi ProductImage
        if (product.ProductImages.Any())
        {
            _context.ProductImages.RemoveRange(
                product.ProductImages
            );
        }

        // Xóa Product
        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        // Sau khi database xóa thành công
        // mới xóa file vật lý
        foreach (var imageUrl in imageUrls)
        {
            TryDeleteImage(imageUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    // =====================================================
    // KIỂM TRA PRODUCT
    // =====================================================

    private bool ProductExists(int id)
    {
        return _context.Products
            .Any(p => p.Id == id);
    }

    // =====================================================
    // LOAD BRAND + CATEGORY
    // =====================================================

    private void LoadDropdowns(
        int? brandId = null,
        int? categoryId = null)
    {
        ViewBag.BrandId = new SelectList(
            _context.Brands,
            "Id",
            "Name",
            brandId
        );

        ViewBag.CategoryId = new SelectList(
            _context.Categories,
            "Id",
            "Name",
            categoryId
        );
    }

    // =====================================================
    // LƯU ẢNH
    // =====================================================

    private async Task<string> SaveImage(
        IFormFile imageFile)
    {
        string uploadFolder = Path.Combine(
            _webHostEnvironment.WebRootPath,
            "images",
            "products"
        );

        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(
                uploadFolder
            );
        }

        string extension =
            Path.GetExtension(
                imageFile.FileName
            ).ToLowerInvariant();

        // Chỉ cho phép một số loại file ảnh
        string[] allowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Chỉ cho phép file JPG, JPEG, PNG hoặc WEBP."
            );
        }

        string fileName =
            $"{Guid.NewGuid()}{extension}";

        string filePath =
            Path.Combine(
                uploadFolder,
                fileName
            );

        await using (
            FileStream stream =
                new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None
                ))
        {
            await imageFile.CopyToAsync(
                stream
            );
        }

        return
            $"/images/products/{fileName}";
    }

    // =====================================================
    // XÓA ẢNH VẬT LÝ
    // =====================================================

    private void TryDeleteImage(
        string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(
            imageUrl))
        {
            return;
        }

        if (!imageUrl.StartsWith(
            "/images/products/"))
        {
            return;
        }

        string fileName =
            Path.GetFileName(imageUrl);

        string filePath =
            Path.Combine(
                _webHostEnvironment.WebRootPath,
                "images",
                "products",
                fileName
            );

        try
        {
            if (System.IO.File.Exists(
                filePath))
            {
                System.IO.File.Delete(
                    filePath
                );
            }
        }
        catch (IOException)
        {
            // File đang được sử dụng:
            // không làm ứng dụng bị crash.
        }
        catch (UnauthorizedAccessException)
        {
            // Không đủ quyền xóa:
            // không làm ứng dụng bị crash.
        }
    }
}