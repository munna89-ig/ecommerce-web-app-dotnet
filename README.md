# Real time E-commerce Web App

A full-featured online store for jewellery and fashion accessories, built with **ASP.NET Core 8 MVC** and **SQL Server**, and deployed on **Microsoft Azure**.

🔗 **Live demo:** https://online-shop-cwedfeg3a9dgc2h3.centralindia-01.azurewebsites.net/

> Hosted on the Azure free tier, so the first page load after a period of inactivity can take 10–20 seconds while the app wakes up.

---

## Table of contents
- [Features](#features)
- [Tech stack](#tech-stack)
- [Architecture](#architecture)
- [Folder structure](#folder-structure)
- [Database](#database)
- [Running locally](#running-locally)
- [Deployment](#deployment)
- [Author](#author)

---

## Features

### Customer side
- Browse products by category, search, and view product details
- Product variants (colour / size) with their own price and stock
- Shopping cart for both logged-in users and guests
- Wishlist
- Checkout with saved shipping addresses
- Coupon / discount codes
- Payment by **bKash** (manual verification) or **Cash on Delivery**
- Order history, order tracking and return requests
- Product reviews and ratings
- Registration, login and profile management (photo, name, phone, password)
- Dark mode and responsive design for mobile, tablet and desktop

### Admin panel
- Dashboard and sales analytics
- Product management (create, edit, variants, images, featured items)
- Category and homepage banner management
- Inventory tracking with stock movement history
- Order management with status history
- Payment verification (approve or reject bKash payments)
- Returns, reviews and coupon management
- User management (add, edit, activate/deactivate, reset password, delete)
- Audit log of important actions (logins, payments, data changes)
- Site settings
- PDF invoices

---

## Tech stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 MVC, C# |
| Database | SQL Server, Entity Framework Core 8 |
| Frontend | Razor Views, HTML, CSS, JavaScript |
| Admin UI | Tailwind CSS 4, daisyUI 5 |
| PDF | QuestPDF |
| Auth | Cookie authentication, role-based (Customer / Seller / Admin), PBKDF2-SHA256 password hashing |
| Hosting | Azure App Service (Windows), Azure SQL Database |

---

## Architecture

The app follows a **layered MVC architecture**. Controllers stay thin and pass the work to services; services contain the business rules and talk to the database through Entity Framework Core.

```
                ┌──────────────────────────────────────────┐
  Browser  ───► │  Controllers  (Controllers/, Areas/Admin)│
                │  - routing, validation, auth checks      │
                └───────────────┬──────────────────────────┘
                                │ ViewModels
                ┌───────────────▼──────────────────────────┐
                │  Services  (Services/)                   │
                │  - cart, checkout, orders, payments,     │
                │    coupons, inventory, reviews, PDF      │
                └───────────────┬──────────────────────────┘
                                │ Entities
                ┌───────────────▼──────────────────────────┐
                │  Data  (EcommerceSystemContext + EF Core)│
                │  - AuditInterceptor logs every change    │
                └───────────────┬──────────────────────────┘
                                │
                        ┌───────▼───────┐
                        │  SQL Server   │
                        └───────────────┘

   Views (Razor) ◄── Controllers return ViewModels to the views
   Helpers ────────► used by every layer (hashing, uploads, slugs, prices)
```

**Key design points**
- **Two areas:** the customer site lives in the root `Controllers/` + `Views/`, and the admin panel lives in `Areas/Admin/` with its own layout. Admin routes are protected with `[Authorize(Roles = "Admin")]`.
- **Dependency injection:** every service is registered in `Program.cs` behind an interface (`IAccountService`, `ICartService`, …).
- **ViewModels:** views never receive raw database entities for forms; each page has its own ViewModel with validation attributes.
- **Guest shoppers:** `ShopperResolver` lets guests use the cart and checkout without an account.
- **Auditing:** `AuditInterceptor` hooks into EF Core and records creates, updates and deletes automatically; `AuditLogger` records events such as logins and payment decisions.
- **Configuration:** the database connection string is read from `ConnectionStrings:ecommerce`, which Azure overrides in production.

### Routing
| Route | Goes to |
|---|---|
| `/{controller=Home}/{action=Index}/{id?}` | Customer site |
| `/Admin/{controller=Dashboard}/{action=Index}/{id?}` | Admin panel |

---

## Folder structure

```
ecommerce/
│
├── Pearl Poetry.sln                     # Visual Studio solution
├── Pearl_Poetry.csproj                  # Project file and NuGet packages
├── Program.cs                           # App startup, DI, auth, routing
├── appsettings.json                     # Configuration (connection string)
├── package.json                         # Tailwind / daisyUI build for admin CSS
├── README.md
│
├── Areas/
│   └── Admin/                           # ===== ADMIN PANEL =====
│       ├── Controllers/
│       │   ├── AnalyticsController.cs       # Sales charts and reports
│       │   ├── AuditLogController.cs        # Activity history
│       │   ├── BannerController.cs          # Homepage banners
│       │   ├── CategoryController.cs        # Category CRUD (AJAX)
│       │   ├── CouponController.cs          # Discount codes
│       │   ├── DashboardController.cs       # Admin home page
│       │   ├── InventoryController.cs       # Stock levels and movements
│       │   ├── OrderController.cs           # Order list, status updates
│       │   ├── PaymentController.cs         # Verify / reject bKash payments
│       │   ├── ProductController.cs         # Product CRUD, variants, images
│       │   ├── ReturnController.cs          # Return requests
│       │   ├── ReviewController.cs          # Review moderation
│       │   ├── SettingsController.cs        # Site name, contact info
│       │   └── UserController.cs            # User management
│       ├── Views/
│       │   ├── Analytics/       Index.cshtml
│       │   ├── AuditLog/        Index.cshtml, Details.cshtml
│       │   ├── Banner/          Index.cshtml, Form.cshtml
│       │   ├── Category/        Index.cshtml
│       │   ├── Coupon/          Index.cshtml, Create.cshtml, Edit.cshtml
│       │   ├── Dashboard/       Index.cshtml
│       │   ├── Inventory/       Index.cshtml
│       │   ├── Order/           Index.cshtml, Details.cshtml
│       │   ├── Payment/         Index.cshtml
│       │   ├── Product/         Index.cshtml, AddProduct.cshtml,
│       │   │                    EditProduct.cshtml, Details.cshtml
│       │   ├── Return/          Index.cshtml
│       │   ├── Review/          Index.cshtml
│       │   ├── Settings/        Index.cshtml
│       │   ├── User/            Index.cshtml, Create.cshtml, Edit.cshtml,
│       │   │                    Details.cshtml, Delete.cshtml, ResetPassword.cshtml
│       │   └── Shared/          _AdminLayout.cshtml
│       ├── _ViewImports.cshtml
│       └── _ViewStart.cshtml
│
├── Controllers/                         # ===== CUSTOMER SITE =====
│   ├── AccountController.cs             # Login, register, logout, profile
│   ├── CartController.cs                # Add / update / remove cart items
│   ├── CategoryController.cs            # Category listing and pages
│   ├── CheckoutController.cs            # Shipping → payment → success
│   ├── CouponController.cs              # Apply coupon at checkout
│   ├── HomeController.cs                # Home page
│   ├── OrderController.cs               # My orders, order details
│   ├── PaymentController.cs             # bKash payment submission
│   ├── ProductController.cs             # Product list, details, search
│   ├── ReviewController.cs              # Write product reviews
│   ├── ShippingAddressController.cs     # Saved addresses
│   └── WishlistController.cs            # Wishlist toggle and page
│
├── Data/
│   ├── EcommerceSystemContext.cs        # EF Core DbContext
│   └── Entities/                        # One class per database table
│       ├── AuditLog.cs          ├── OrderStatusHistory.cs
│       ├── Banner.cs            ├── Payment.cs
│       ├── CartItem.cs          ├── Product.cs
│       ├── Category.cs          ├── ProductImage.cs
│       ├── Coupon.cs            ├── ProductReview.cs
│       ├── CouponUsage.cs       ├── ProductVariant.cs
│       ├── GuestUser.cs         ├── Setting.cs
│       ├── InventoryMovement.cs ├── ShippingAddress.cs
│       ├── Order.cs             ├── User.cs
│       ├── OrderItem.cs         └── Wishlist.cs
│       └── OrderReturn.cs
│
├── Helpers/
│   ├── AuditInterceptor.cs              # Logs EF Core changes automatically
│   ├── AuditLogger.cs                   # Logs custom events (login, payment)
│   ├── FileUploadHelper.cs              # Safe image upload / delete
│   ├── OrderNumberGenerator.cs          # Order numbers like PP-261030-706479
│   ├── PasswordHelper.cs                # PBKDF2 hashing and verification
│   ├── PriceCalculator.cs               # Discounts, totals
│   ├── Shopperresolver.cs               # Logged-in user or guest
│   ├── SiteInfo.cs                      # Site name / settings for views
│   ├── SlugHelper.cs                    # URL-friendly names
│   ├── UserRoleHelper.cs                # Role checks
│   └── VariantHelper.cs                 # Variant price and stock
│
├── Mapping/                             # Reserved for mapping profiles (currently empty)
│
├── Models/
│   ├── Enums/
│   │   ├── InventoryReason.cs   ├── PaymentStatus.cs
│   │   ├── OrderStatus.cs       ├── ProductType.cs
│   │   ├── PaymentMethod.cs     ├── ReturnStatus.cs
│   │   └── UserRole.cs                  # Customer = 1, Seller = 2, Admin = 3
│   ├── ErrorViewModel.cs
│   └── ViewModels/
│       ├── Account/     LoginViewModel, RegisterViewModel, ProfileViewModel
│       ├── Admin/       Analytics, AuditLog, BannerForm, Dashboard, Inventory,
│       │                PaymentAdmin, RecentActivity, RecentOrder, ResetPassword,
│       │                SalesChartItem, Settings, UserCreate, UserDetails,
│       │                UserEdit, UserList
│       ├── Cart/        CartViewModel, CartItemViewModel
│       ├── Checkout/    CheckoutViewModel, ShippingViewModel, PaymentViewModel
│       ├── Coupon/      CouponCreateViewModel, CouponEditViewModel
│       ├── Order/       OrderListViewModel, OrderDetailsViewModel
│       ├── Product/     ProductCreate, ProductEdit, ProductDetails, ProductList
│       └── Shop/        ProductSearch, ShopProductDetails, Wishlist,
│                        WishlistToggleResult
│
├── Services/
│   ├── Interfaces/                      # IAccountService, ICartService, ICategoryService,
│   │                                    # ICheckoutService, ICouponService, IInventoryService,
│   │                                    # IOrderService, IPaymentService, IProductService,
│   │                                    # IReviewService, IShippingService, IWishlistService
│   ├── AccountService.cs                # Login, register, profile, password
│   ├── CartService.cs                   # Cart logic for users and guests
│   ├── CategoryService.cs
│   ├── CheckoutService.cs               # Builds and places orders
│   ├── CouponService.cs                 # Coupon validation and usage
│   ├── InventoryService.cs              # Stock in / out
│   ├── InvoicePdfService.cs             # PDF invoices (QuestPDF)
│   ├── OrderService.cs                  # Order queries and status changes
│   ├── PaymentService.cs                # Payment verify / reject
│   ├── ProductService.cs
│   ├── ReviewService.cs
│   ├── ShippingService.cs
│   └── WishlistService.cs
│
├── Views/                               # Customer-facing Razor views
│   ├── Account/     Login.cshtml, Register.cshtml, Profile.cshtml
│   ├── Cart/        Index.cshtml
│   ├── Category/    Index.cshtml, Details.cshtml
│   ├── Checkout/    Index.cshtml, Shipping.cshtml, Payment.cshtml, Success.cshtml
│   ├── Home/        Index.cshtml, Privacy.cshtml
│   ├── Order/       Index.cshtml, Details.cshtml
│   ├── Product/     Index.cshtml, Details.cshtml, Search.cshtml
│   ├── Wishlist/    Index.cshtml
│   ├── Shared/      _Layout.cshtml, _Navbar.cshtml, _Footer.cshtml,
│   │                _ProductCard.cshtml, _ValidationScriptsPartial.cshtml,
│   │                Error.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
│
├── Properties/
│   └── launchSettings.json              # Local run profiles
│
└── wwwroot/                             # Static files
    ├── css/
    │   ├── site.css                     # Compiled Tailwind + daisyUI (admin)
    │   ├── input.css                    # Tailwind source
    │   ├── Home.css, Shop.css, AllProducts.css, search.css
    │   ├── Category.css, coupon.css, admin.css, users.css
    │   └── login.css, profile.css
    ├── js/
    │   ├── site.js, Home.js, product.js, product-details.js
    │   ├── cart.js, checkout.js, coupon.js
    │   └── admin.js, addProduct.js, Category.js
    ├── images/
    │   ├── banner/                      # Homepage banners
    │   ├── categories/                  # Category images
    │   ├── products/                    # Product images
    │   ├── payments/                    # bKash QR code
    │   ├── users/                       # Profile photos
    │   └── logo.png
    ├── uploads/
    ├── lib/                             # jQuery, jQuery Validation, Bootstrap
    └── favicon.ico
```

---

## Database

SQL Server database `ecommerce_system` with 21 tables. Main relationships:

```
users ──┬── orders ──┬── order_items ── products ──┬── product_variants
        │            ├── payments                  ├── product_images
        │            ├── order_status_histories    ├── product_reviews
        │            └── order_returns             └── categories
        ├── cart_items ─────────────── products
        ├── wishlist ───────────────── products
        ├── shipping_addresses
        └── audit_logs

coupons ──┬── coupon_categories ── categories
          └── coupon_usages ────── users / orders

guest_users ── cart_items / orders      (shopping without an account)
inventory_movements ── products / variants
banners, settings
```

- Deleting a product or category cascades to its images, variants, reviews, cart and wishlist rows.
- Orders keep their own copy of prices, so later price changes do not affect past orders.

---

## Running locally

### Requirements
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Express is enough) and SQL Server Management Studio
- Visual Studio 2022 (or VS Code)
- Node.js (only to rebuild the admin CSS)

### Steps
1. **Clone the repository**
   ```bash
   git clone https://github.com/munna89-ig/ecommerce-web-app-dotnet.git
   cd ecommerce-web-app-dotnet/ecommerce
   ```

2. **Set up the database**
   Create a database named `ecommerce_system` in SQL Server and restore the schema and data.

3. **Update the connection string** in `appsettings.json`
   ```json
   "ConnectionStrings": {
     "ecommerce": "Data Source=YOUR_SERVER\\SQLEXPRESS;Initial Catalog=ecommerce_system;TrustServerCertificate=True;Integrated Security=True;"
   }
   ```

4. **Run**
   ```bash
   dotnet run
   ```
   Or open `Pearl Poetry.sln` in Visual Studio and press **F5**.

5. **(Optional) Rebuild the admin CSS**
   ```bash
   npm install
   npm run build      # or: npm run dev  (watch mode)
   ```

---

## Deployment

| Part | Azure service | Region |
|---|---|---|
| Web app | Azure App Service (Windows, .NET 8, Free F1) | Central India |
| Database | Azure SQL Database (Basic) | Central India |

- The production connection string lives in **App Service → Environment variables → Connection strings** (name `ecommerce`, type `SQLAzure`). No secrets are stored in the code.
- New versions are published from Visual Studio with **Publish → Azure App Service**.
- Database changes are applied to Azure SQL with SSMS.

---

## Author

**Munna** — [@munna89-ig](https://github.com/munna89-ig)

Built as a university project. All rights reserved — please ask before using any part of this code.
