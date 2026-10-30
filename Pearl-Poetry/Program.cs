using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Helpers;
using Pearl_Poetry.Interfaces;
using Pearl_Poetry.Services;
using Pearl_Poetry.Services.Interfaces;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;

namespace Pearl_Poetry
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ---------- QuestPDF (invoice) ----------
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.UseSystemFonts = true;

            // fonts that support Bengali characters
            var fontPath = Path.Combine(AppContext.BaseDirectory, "fonts");
            if (Directory.Exists(fontPath))
            {
                foreach (var fontFile in Directory.GetFiles(fontPath, "*.ttf"))
                {
                    FontManager.RegisterFontFromFile(fontFile);
                }
            }

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // ---------- database ----------
            var cs = builder.Configuration.GetConnectionString("ecommerce");
            // every insert / update / delete is written to audit_logs automatically
            builder.Services.AddScoped<AuditInterceptor>();
            builder.Services.AddDbContext<EcommerceSystemContext>((sp, opt) =>
                opt.UseSqlServer(cs)
                   .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

            builder.Services.AddScoped<IAuditLogger, AuditLogger>();

            // ---------- auth ----------
            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                });

            builder.Services.AddAuthorization();

            // ---------- services ----------
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICouponService, CouponService>();   // <- this was missing
            builder.Services.AddScoped<InvoicePdfService>();

            // ---------- shop (cart / wishlist / checkout) ----------
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IShopperResolver, ShopperResolver>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IWishlistService, WishlistService>();
            builder.Services.AddScoped<ICheckoutService, CheckoutService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<SiteInfo>();
          

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}