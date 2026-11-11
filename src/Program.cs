using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Repositories.Admin;
using MiaraEyeglasses.Repositories.Auth;
using MiaraEyeglasses.Repositories.Common;
using MiaraEyeglasses.Repositories.Order;
using MiaraEyeglasses.Repositories.Product;
using MiaraEyeglasses.Repositories.Staff;
using MiaraEyeglasses.Repositories.User;
using MiaraEyeglasses.Services.Admin;
using MiaraEyeglasses.Services.Auth;
using MiaraEyeglasses.Services.Common;
using MiaraEyeglasses.Services.Order;
using MiaraEyeglasses.Services.Product;
using MiaraEyeglasses.Services.Staff;
using MiaraEyeglasses.Services.User;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DbContext with SQL Server
builder.Services.AddDbContext<EyeglassesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EyeglassesShopConnection")));

// 2. Add Authentication with Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// 3. Add Session and HttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. Register Repositories (Data Access Layer - 1 Feature = 1 Repository)
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

// 4. Register Repositories (1 Feature = 1 Repository)
// Auth Repositories
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<IRegisterRepository, RegisterRepository>();
builder.Services.AddScoped<IForgotPasswordRepository, ForgotPasswordRepository>();
builder.Services.AddScoped<IResetPasswordRepository, ResetPasswordRepository>();

// User Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IChangePasswordRepository, ChangePasswordRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<IUserOrderRepository, UserOrderRepository>();

// Product & Catalog Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ILensRepository, LensRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// Order & Cart Repositories
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();

// Staff Repositories
builder.Services.AddScoped<IStaffOrderRepository, StaffOrderRepository>();
builder.Services.AddScoped<IStaffInventoryRepository, StaffInventoryRepository>();
builder.Services.AddScoped<IStaffReviewRepository, StaffReviewRepository>();

// Admin Repositories
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<IAdminOrderRepository, AdminOrderRepository>();
builder.Services.AddScoped<IAdminProductRepository, AdminProductRepository>();
builder.Services.AddScoped<IAdminCategoryRepository, AdminCategoryRepository>();
builder.Services.AddScoped<IAdminBrandRepository, AdminBrandRepository>();
builder.Services.AddScoped<IAdminLensRepository, AdminLensRepository>();
builder.Services.AddScoped<IAdminCouponRepository, AdminCouponRepository>();
builder.Services.AddScoped<IAdminBannerRepository, AdminBannerRepository>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();

// 5. Register Services (Business Logic Layer - 1 Feature = 1 Service)
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// Auth Services
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ILogoutService, LogoutService>();
builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
builder.Services.AddScoped<IResetPasswordService, ResetPasswordService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// User Services
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IChangePasswordService, ChangePasswordService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IUserOrderService, UserOrderService>();

// Product Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ILensService, LensService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// Order & Cart Services
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();

// Staff Services
builder.Services.AddScoped<IStaffOrderService, StaffOrderService>();
builder.Services.AddScoped<IStaffInventoryService, StaffInventoryService>();
builder.Services.AddScoped<IStaffReviewService, StaffReviewService>();

// Admin Services
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
builder.Services.AddScoped<IAdminProductService, AdminProductService>();
builder.Services.AddScoped<IAdminCategoryService, AdminCategoryService>();
builder.Services.AddScoped<IAdminBrandService, AdminBrandService>();
builder.Services.AddScoped<IAdminLensService, AdminLensService>();
builder.Services.AddScoped<IAdminCouponService, AdminCouponService>();
builder.Services.AddScoped<IAdminBannerService, AdminBannerService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();

// 6. Add MVC Controllers & Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// Tự động nhận diện danh tính và vai trò người dùng từ Session
app.Use(async (context, next) =>
{
    var userId = context.Session.GetString("UserId");
    var userName = context.Session.GetString("UserName");
    var roleName = context.Session.GetString("RoleName");
    if (!string.IsNullOrEmpty(userId))
    {
        var identity = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, userName ?? ""),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, roleName ?? "Customer")
        }, "SessionAuth");
        context.User = new System.Security.Claims.ClaimsPrincipal(identity);
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
