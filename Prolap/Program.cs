using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProLap.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// DATABASE
// ==========================================
builder.Services.AddDbContext<ProLapDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ==========================================
// IDENTITY
// ==========================================
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        // Tạm thời đặt yêu cầu mật khẩu đơn giản
        // để thuận tiện khi phát triển đồ án
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;

        // Không bắt buộc xác nhận email
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ProLapDbContext>();

// ==========================================
// MVC
// ==========================================
builder.Services.AddControllersWithViews();

// ==========================================
// RAZOR PAGES
// ==========================================
builder.Services.AddRazorPages();

// ==========================================
// SESSION
// ==========================================
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ==========================================
// HTTP PIPELINE
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// ==========================================
// AUTHENTICATION + AUTHORIZATION
// ==========================================
app.UseAuthentication();

app.UseAuthorization();

// ==========================================
// SESSION
// ==========================================
app.UseSession();

app.MapStaticAssets();

// ==========================================
// MVC ROUTE
// ==========================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// ==========================================
// IDENTITY RAZOR PAGES
// ==========================================
app.MapRazorPages();

// ==========================================
// TẠO ROLE ADMIN + TÀI KHOẢN ADMIN MẶC ĐỊNH
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Tạo role Admin nếu chưa tồn tại
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(
            new IdentityRole("Admin"));
    }

    // ==========================================
    // TÀI KHOẢN ADMIN MẶC ĐỊNH
    // ==========================================
    string adminEmail = "admin@prolap.com";
    string adminPassword = "Admin123";

    var adminUser =
        await userManager.FindByEmailAsync(adminEmail);

    // Nếu tài khoản chưa tồn tại thì tạo mới
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult =
            await userManager.CreateAsync(
                adminUser,
                adminPassword);

        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(
                adminUser,
                "Admin");
        }
    }
    else
    {
        // Nếu tài khoản đã có nhưng chưa có role Admin
        if (!await userManager.IsInRoleAsync(
                adminUser,
                "Admin"))
        {
            await userManager.AddToRoleAsync(
                adminUser,
                "Admin");
        }
    }
}

app.Run();