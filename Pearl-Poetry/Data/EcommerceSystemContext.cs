using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data.Entities;

namespace Pearl_Poetry.Data;

public partial class EcommerceSystemContext : DbContext
{
    public EcommerceSystemContext()
    {
    }

    public EcommerceSystemContext(DbContextOptions<EcommerceSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Banner> Banners { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<CouponUsage> CouponUsages { get; set; }

    public virtual DbSet<GuestUser> GuestUsers { get; set; }

    public virtual DbSet<InventoryMovement> InventoryMovements { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderReturn> OrderReturns { get; set; }

    public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<ShippingAddress> ShippingAddresses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ecommerce");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__audit_lo__3213E83F84F774D6");

            entity.ToTable("audit_logs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(255)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(100)
                .HasColumnName("entity_type");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.NewValues).HasColumnName("new_values");
            entity.Property(e => e.OldValues).HasColumnName("old_values");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_audit_user");
        });

        modelBuilder.Entity<Banner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__banners__3213E83F7FCA666A");

            entity.ToTable("banners", tb => tb.HasTrigger("trg_banners_updated_at"));

            entity.HasIndex(e => new { e.Position, e.IsActive }, "idx_banners_position_active");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("image_path");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LinkUrl)
                .HasMaxLength(255)
                .HasColumnName("link_url");
            entity.Property(e => e.Position)
                .HasMaxLength(30)
                .HasDefaultValue("homepage_hero")
                .HasColumnName("position");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Banners)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_banners_created_by");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cart_ite__3213E83F9198E127");

            entity.ToTable("cart_items");

            entity.HasIndex(e => e.GuestUserId, "idx_cart_guest");

            entity.HasIndex(e => e.UserId, "idx_cart_user");

            entity.HasIndex(e => new { e.GuestUserId, e.ProductId, e.VariantId }, "uq_cart_guest_product_variant").IsUnique();

            entity.HasIndex(e => new { e.UserId, e.ProductId, e.VariantId }, "uq_cart_user_product_variant").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.GuestUserId).HasColumnName("guest_user_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");

            entity.HasOne(d => d.GuestUser).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.GuestUserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_cart_guest");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("fk_cart_product");

            entity.HasOne(d => d.User).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_cart_user");

            entity.HasOne(d => d.Variant).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("fk_cart_variant");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__categori__3213E83FFA4C0C08");

            entity.ToTable("categories", tb => tb.HasTrigger("trg_categories_delete_parent_cleanup"));

            entity.HasIndex(e => e.Slug, "UQ__categori__32DD1E4C640C8320").IsUnique();

            entity.HasIndex(e => e.IsActive, "idx_categories_is_active");

            entity.HasIndex(e => e.ParentId, "idx_categories_parent");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("image_path");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Slug)
                .HasMaxLength(120)
                .HasColumnName("slug");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("fk_categories_parent");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__coupons__3213E83FB00131A3");

            entity.ToTable("coupons");

            entity.HasIndex(e => e.Code, "UQ__coupons__357D4CF923E8B612").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(20)
                .HasDefaultValue("percentage")
                .HasColumnName("discount_type");
            entity.Property(e => e.DiscountValue)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("discount_value");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MaxDiscountAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("max_discount_amount");
            entity.Property(e => e.MaxUsage)
                .HasDefaultValue(1)
                .HasColumnName("max_usage");
            entity.Property(e => e.MaxUsagePerUser)
                .HasDefaultValue(1)
                .HasColumnName("max_usage_per_user");
            entity.Property(e => e.MinOrderAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("min_order_amount");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.UsedCount)
                .HasDefaultValue(0)
                .HasColumnName("used_count");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Coupons)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_coupons_user");
            entity.Property(e => e.MinCustomerSpend)
            .HasColumnType("decimal(10, 2)")
            .HasColumnName("min_customer_spend");

            entity.HasMany(d => d.Categories).WithMany(p => p.Coupons)
                .UsingEntity<Dictionary<string, object>>(
                    "CouponCategory",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__coupon_ca__categ__503BEA1C"),
                    l => l.HasOne<Coupon>().WithMany()
                        .HasForeignKey("CouponId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__coupon_ca__coupo__4F47C5E3"),
                    j =>
                    {
                        j.HasKey("CouponId", "CategoryId").HasName("PK__coupon_c__059B8D12DC6F2B7B");
                        j.ToTable("coupon_categories");
                        j.IndexerProperty<int>("CouponId").HasColumnName("coupon_id");
                        j.IndexerProperty<int>("CategoryId").HasColumnName("category_id");
                    });
        });

        modelBuilder.Entity<CouponUsage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__coupon_u__3213E83FC273AAE2");

            entity.ToTable("coupon_usages");

            entity.HasIndex(e => new { e.CouponId, e.UserId }, "idx_coupon_usages_coupon_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.GuestUserId).HasColumnName("guest_user_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.UsedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("used_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Coupon).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.CouponId)
                .HasConstraintName("fk_cu_coupon");

            entity.HasOne(d => d.GuestUser).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.GuestUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_cu_guest");

            entity.HasOne(d => d.Order).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cu_order");

            entity.HasOne(d => d.User).WithMany(p => p.CouponUsages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_cu_user");
        });

        modelBuilder.Entity<GuestUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__guest_us__3213E83FD02D323E");

            entity.ToTable("guest_users");

            entity.HasIndex(e => e.SessionId, "UQ__guest_us__69B13FDD17B015A0").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.SessionId)
                .HasMaxLength(191)
                .HasColumnName("session_id");
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__inventor__3213E83F0733A278");

            entity.ToTable("inventory_movements");

            entity.HasIndex(e => e.ProductId, "idx_inventory_product");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChangeQty).HasColumnName("change_qty");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Reason)
                .HasMaxLength(20)
                .HasColumnName("reason");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(50)
                .HasColumnName("reference_type");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_inv_created_by");

            entity.HasOne(d => d.Product).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("fk_inv_product");

            entity.HasOne(d => d.Variant).WithMany(p => p.InventoryMovements)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("fk_inv_variant");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orders__3213E83FBAD0925C");

            entity.ToTable("orders");

            entity.HasIndex(e => e.OrderNumber, "UQ__orders__730E34DF8B4DCB11").IsUnique();

            entity.HasIndex(e => e.GuestUserId, "idx_orders_guest_user_id");

            entity.HasIndex(e => e.OrderNumber, "idx_orders_order_number");

            entity.HasIndex(e => e.PaymentStatus, "idx_orders_payment_status");

            entity.HasIndex(e => e.Status, "idx_orders_status");

            entity.HasIndex(e => e.UserId, "idx_orders_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CouponCode)
                .HasMaxLength(50)
                .HasColumnName("coupon_code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerNote).HasColumnName("customer_note");
            entity.Property(e => e.DeliveryCharge)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("delivery_charge");
            entity.Property(e => e.DiscountAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("discount_amount");
            entity.Property(e => e.GuestUserId).HasColumnName("guest_user_id");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(30)
                .HasColumnName("order_number");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(25)
                .HasDefaultValue("unpaid")
                .HasColumnName("payment_status");
            entity.Property(e => e.ShipAddress).HasColumnName("ship_address");
            entity.Property(e => e.ShipCountry)
                .HasMaxLength(100)
                .HasDefaultValue("Bangladesh")
                .HasColumnName("ship_country");
            entity.Property(e => e.ShipDeliveryNote).HasColumnName("ship_delivery_note");
            entity.Property(e => e.ShipDistrict)
                .HasMaxLength(100)
                .HasColumnName("ship_district");
            entity.Property(e => e.ShipEmail)
                .HasMaxLength(100)
                .HasColumnName("ship_email");
            entity.Property(e => e.ShipFullName)
                .HasMaxLength(100)
                .HasColumnName("ship_full_name");
            entity.Property(e => e.ShipPhone)
                .HasMaxLength(20)
                .HasColumnName("ship_phone");
            entity.Property(e => e.ShipPostalCode)
                .HasMaxLength(20)
                .HasColumnName("ship_postal_code");
            entity.Property(e => e.ShippingAddressId).HasColumnName("shipping_address_id");
            entity.Property(e => e.Status)
                .HasMaxLength(25)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.Subtotal)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.GuestUser).WithMany(p => p.Orders)
                .HasForeignKey(d => d.GuestUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_orders_guest");

            entity.HasOne(d => d.ShippingAddress).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ShippingAddressId)
                .HasConstraintName("fk_orders_shipping_address");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_orders_user");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__order_it__3213E83FC4D3B3F9");

            entity.ToTable("order_items");

            entity.HasIndex(e => e.OrderId, "idx_order_items_order");

            entity.HasIndex(e => e.ProductId, "idx_order_items_product");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductTitle)
                .HasMaxLength(255)
                .HasColumnName("product_title");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");
            entity.Property(e => e.VariantLabel)
                .HasMaxLength(150)
                .HasColumnName("variant_label");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_order_items_order");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_order_items_product");

            entity.HasOne(d => d.Variant).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("fk_order_items_variant");
        });

        modelBuilder.Entity<OrderReturn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__order_re__3213E83F314E0580");

            entity.ToTable("order_returns");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.GuestUserId).HasColumnName("guest_user_id");
            entity.Property(e => e.HandledBy).HasColumnName("handled_by");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.RefundAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("refund_amount");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("requested")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.GuestUser).WithMany(p => p.OrderReturns)
                .HasForeignKey(d => d.GuestUserId)
                .HasConstraintName("fk_return_guest");

            entity.HasOne(d => d.HandledByNavigation).WithMany(p => p.OrderReturnHandledByNavigations)
                .HasForeignKey(d => d.HandledBy)
                .HasConstraintName("fk_return_handled_by");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderReturns)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_return_order");

            entity.HasOne(d => d.OrderItem).WithMany(p => p.OrderReturns)
                .HasForeignKey(d => d.OrderItemId)
                .HasConstraintName("fk_return_order_item");

            entity.HasOne(d => d.User).WithMany(p => p.OrderReturnUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_return_user");
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__order_st__3213E83FEB219CEF");

            entity.ToTable("order_status_history");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.ChangedBy)
                .HasConstraintName("fk_osh_changed_by");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_osh_order");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__payments__3213E83F05F22E8E");

            entity.ToTable("payments");

            entity.HasIndex(e => e.OrderId, "idx_payments_order");

            entity.HasIndex(e => e.Status, "idx_payments_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.GuestUserId).HasColumnName("guest_user_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(20)
                .HasDefaultValue("bkash")
                .HasColumnName("payment_method");
            entity.Property(e => e.SenderName)
                .HasMaxLength(100)
                .HasColumnName("sender_name");
            entity.Property(e => e.SenderPhone)
                .HasMaxLength(20)
                .HasColumnName("sender_phone");
            entity.Property(e => e.Status)
                .HasMaxLength(25)
                .HasDefaultValue("pending_verification")
                .HasColumnName("status");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(150)
                .HasColumnName("transaction_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VerificationNote).HasColumnName("verification_note");
            entity.Property(e => e.VerifiedBy).HasColumnName("verified_by");

            entity.HasOne(d => d.GuestUser).WithMany(p => p.Payments)
                .HasForeignKey(d => d.GuestUserId)
                .HasConstraintName("fk_payment_guest");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_payment_order");

            entity.HasOne(d => d.User).WithMany(p => p.PaymentUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_payment_user");

            entity.HasOne(d => d.VerifiedByNavigation).WithMany(p => p.PaymentVerifiedByNavigations)
                .HasForeignKey(d => d.VerifiedBy)
                .HasConstraintName("fk_payment_verified_by");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__products__3213E83FCC2205BB");

            entity.ToTable("products", tb => tb.HasTrigger("trg_products_updated_at"));

            entity.HasIndex(e => e.Slug, "UQ__products__32DD1E4CE831DBF7").IsUnique();

            entity.HasIndex(e => e.Sku, "UQ__products__DDDF4BE7FF00C566").IsUnique();

            entity.HasIndex(e => new { e.CategoryId, e.IsActive }, "idx_products_category_active");

            entity.HasIndex(e => e.IsActive, "idx_products_is_active");

            entity.HasIndex(e => new { e.Sku, e.IsActive }, "idx_products_sku_active");

            entity.HasIndex(e => e.Slug, "idx_products_slug");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CoverImagePath)
                .HasMaxLength(255)
                .HasColumnName("cover_image_path");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("discount_price");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsFeatured)
                .HasDefaultValue(false)
                .HasColumnName("is_featured");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductType)
                .HasMaxLength(20)
                .HasDefaultValue("other")
                .HasColumnName("product_type");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(0)
                .HasColumnName("quantity");
            entity.Property(e => e.ShortDescription).HasColumnName("short_description");
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .HasColumnName("sku");
            entity.Property(e => e.Slug)
                .HasMaxLength(255)
                .HasColumnName("slug");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_products_category");

            entity.HasMany(d => d.Categories).WithMany(p => p.ProductsNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductCategory",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("fk_pc_category"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .HasConstraintName("fk_pc_product"),
                    j =>
                    {
                        j.HasKey("ProductId", "CategoryId").HasName("pk_product_categories");
                        j.ToTable("product_categories");
                        j.IndexerProperty<int>("ProductId").HasColumnName("product_id");
                        j.IndexerProperty<int>("CategoryId").HasColumnName("category_id");
                    });
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__product___3213E83F9C81900F");

            entity.ToTable("product_images");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("image_path");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("fk_product_images_product");

            entity.HasOne(d => d.Variant).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.VariantId)
                .HasConstraintName("fk_product_images_variant");
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__product___3213E83F6925C702");

            entity.ToTable("product_reviews");

            entity.HasIndex(e => e.IsApproved, "idx_reviews_approved");

            entity.HasIndex(e => e.ProductId, "idx_reviews_product");

            entity.HasIndex(e => e.UserId, "idx_reviews_user");

            entity.HasIndex(e => new { e.ProductId, e.UserId }, "uq_review_product_user").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsApproved)
                .HasDefaultValue(false)
                .HasColumnName("is_approved");
            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Review).HasColumnName("review");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.OrderItem).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.OrderItemId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_review_order_item");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("fk_review_product");

            entity.HasOne(d => d.User).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_review_user");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__product___3213E83F3A2A5628");

            entity.ToTable("product_variants");

            entity.HasIndex(e => e.Sku, "UQ__product___DDDF4BE7E3975429").IsUnique();

            entity.HasIndex(e => e.ProductId, "idx_variants_product");

            entity.HasIndex(e => e.Sku, "idx_variants_sku");

            entity.HasIndex(e => new { e.ProductId, e.Color, e.Size }, "uq_variant_combo").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("discount_price");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(0)
                .HasColumnName("quantity");
            entity.Property(e => e.Size)
                .HasMaxLength(50)
                .HasColumnName("size");
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .HasColumnName("sku");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("fk_variant_product");
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__settings__3213E83F51A8B048");

            entity.ToTable("settings");

            entity.Property(e => e.Id)
                .HasDefaultValue(1)
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(300)
                .HasColumnName("address");
            entity.Property(e => e.AdminEmail)
                .HasMaxLength(255)
                .HasColumnName("admin_email");
            entity.Property(e => e.BkashNumber)
                .HasMaxLength(20)
                .HasColumnName("bkash_number");
            entity.Property(e => e.BkashQrImage)
                .HasMaxLength(255)
                .HasColumnName("bkash_qr_image");
            entity.Property(e => e.CopyrightText)
                .HasMaxLength(255)
                .HasColumnName("copyright_text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("BDT")
                .HasColumnName("currency");
            entity.Property(e => e.FacebookUrl)
                .HasMaxLength(255)
                .HasColumnName("facebook_url");
            entity.Property(e => e.InsideDhakaCharge)
                .HasDefaultValue(80.00m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("inside_dhaka_charge");
            entity.Property(e => e.InstagramUrl)
                .HasMaxLength(255)
                .HasColumnName("instagram_url");
            entity.Property(e => e.LogoPath)
                .HasMaxLength(255)
                .HasColumnName("logo_path");
            entity.Property(e => e.MessengerLink)
                .HasMaxLength(255)
                .HasColumnName("messenger_link");
            entity.Property(e => e.OutsideDhakaCharge)
                .HasDefaultValue(150.00m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("outside_dhaka_charge");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.ShippingFee)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("shipping_fee");
            entity.Property(e => e.ShortDescription)
                .HasMaxLength(500)
                .HasColumnName("short_description");
            entity.Property(e => e.SiteName)
                .HasMaxLength(255)
                .HasDefaultValue("E-Commerce System")
                .HasColumnName("site_name");
            entity.Property(e => e.TaxRate)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("tax_rate");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.WhatsappUrl)
                .HasMaxLength(255)
                .HasColumnName("whatsapp_url");
            entity.Property(e => e.YoutubeUrl)
                .HasMaxLength(255)
                .HasColumnName("youtube_url");
        });

        modelBuilder.Entity<ShippingAddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__shipping__3213E83FF97E2230");

            entity.ToTable("shipping_addresses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasDefaultValue("Bangladesh")
                .HasColumnName("country");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeliveryNote).HasColumnName("delivery_note");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .HasColumnName("district");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.GuestUserId).HasColumnName("guest_user_id");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(20)
                .HasColumnName("postal_code");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.GuestUser).WithMany(p => p.ShippingAddresses)
                .HasForeignKey(d => d.GuestUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_shipping_guest");

            entity.HasOne(d => d.User).WithMany(p => p.ShippingAddresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_shipping_user");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F676D73F6");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E6164F441E4E5").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerifiedAt).HasColumnName("email_verified_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.PhoneVerifiedAt).HasColumnName("phone_verified_at");
            entity.Property(e => e.ProfilePic)
                .HasMaxLength(255)
                .HasColumnName("profile_pic");
            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
           
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__wishlist__3213E83F3281FDE9");

            entity.ToTable("wishlist");

            entity.HasIndex(e => e.UserId, "idx_wishlist_user");

            entity.HasIndex(e => new { e.UserId, e.ProductId }, "uq_wishlist_user_product").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Product).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("fk_wishlist_product");

            entity.HasOne(d => d.User).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_wishlist_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
