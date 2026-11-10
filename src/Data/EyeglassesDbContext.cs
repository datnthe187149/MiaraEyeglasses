using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Models.Entities;

namespace MiaraEyeglasses.Data
{
    public class EyeglassesDbContext : DbContext
    {
        public EyeglassesDbContext(DbContextOptions<EyeglassesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserAddress> UserAddresses { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Brand> Brands { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ProductVariant> ProductVariants { get; set; } = null!;
        public virtual DbSet<ProductImage> ProductImages { get; set; } = null!;
        public virtual DbSet<LensOption> LensOptions { get; set; } = null!;
        public virtual DbSet<Coupon> Coupons { get; set; } = null!;
        public virtual DbSet<Banner> Banners { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<CartItem> CartItems { get; set; } = null!;
        public virtual DbSet<CustomerPrescription> CustomerPrescriptions { get; set; } = null!;
        public virtual DbSet<Wishlist> Wishlists { get; set; } = null!;
        public virtual DbSet<OrderStatus> OrderStatuses { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderItem> OrderItems { get; set; } = null!;
        public virtual DbSet<Payment> Payments { get; set; } = null!;
        public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; } = null!;
        public virtual DbSet<Review> Reviews { get; set; } = null!;

        // Database Views
        public virtual DbSet<DailyRevenueReport> DailyRevenues { get; set; } = null!;
        public virtual DbSet<BestSellerReport> BestSellers { get; set; } = null!;
        public virtual DbSet<LowStockReport> LowStocks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Roles
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(e => e.RoleId);
                entity.HasIndex(e => e.RoleName).IsUnique();
            });

            // 2. Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Phone).IsUnique();
                entity.HasIndex(e => new { e.RoleId, e.IsActive });

                entity.HasOne(e => e.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 3. UserAddresses
            modelBuilder.Entity<UserAddress>(entity =>
            {
                entity.ToTable("UserAddresses");
                entity.HasKey(e => e.AddressId);
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Addresses)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 4. Categories
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(e => e.CategoryId);
                entity.HasIndex(e => e.CategoryName).IsUnique();
                entity.HasIndex(e => e.Slug).IsUnique();
            });

            // 5. Brands
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.ToTable("Brands");
                entity.HasKey(e => e.BrandId);
                entity.HasIndex(e => e.BrandName).IsUnique();
            });

            // 6. Products
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.ProductId);
                entity.HasIndex(e => e.ProductCode).IsUnique();
                entity.HasIndex(e => new { e.CategoryId, e.BrandId, e.IsActive });

                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(e => e.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 7. ProductVariants
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.ToTable("ProductVariants");
                entity.HasKey(e => e.VariantId);
                entity.HasIndex(e => e.Sku).IsUnique();
                entity.HasIndex(e => new { e.ProductId, e.ColorName, e.SizeLabel }).IsUnique();

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Variants)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 8. ProductImages
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ProductImages");
                entity.HasKey(e => e.ImageId);
                entity.HasIndex(e => new { e.ProductId, e.SortOrder });

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Images)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Variant)
                      .WithMany(v => v.Images)
                      .HasForeignKey(e => e.VariantId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // 9. LensOptions
            modelBuilder.Entity<LensOption>(entity =>
            {
                entity.ToTable("LensOptions");
                entity.HasKey(e => e.LensOptionId);
                entity.HasIndex(e => e.LensName).IsUnique();
            });

            // 10. Coupons
            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.ToTable("Coupons");
                entity.HasKey(e => e.CouponId);
                entity.HasIndex(e => e.CouponCode).IsUnique();
            });

            // 11. Banners
            modelBuilder.Entity<Banner>(entity =>
            {
                entity.ToTable("Banners");
                entity.HasKey(e => e.BannerId);
            });

            // 12. Notifications
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasKey(e => e.NotificationId);
                entity.HasIndex(e => new { e.TargetRoleId, e.IsRead, e.CreatedAt });
                entity.HasIndex(e => new { e.TargetUserId, e.IsRead, e.CreatedAt });

                entity.HasOne(e => e.TargetUser)
                      .WithMany()
                      .HasForeignKey(e => e.TargetUserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 13. CartItems
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CartItems");
                entity.HasKey(e => e.CartItemId);
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.CartItems)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Variant)
                      .WithMany()
                      .HasForeignKey(e => e.VariantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LensOption)
                      .WithMany()
                      .HasForeignKey(e => e.LensOptionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 14. CustomerPrescriptions
            modelBuilder.Entity<CustomerPrescription>(entity =>
            {
                entity.ToTable("CustomerPrescriptions");
                entity.HasKey(e => e.UserPrescriptionId);
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Prescriptions)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 15. Wishlists (Composite PK)
            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.ToTable("Wishlists");
                entity.HasKey(e => new { e.UserId, e.ProductId });

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Wishlists)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Wishlists)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 16. OrderStatuses
            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.ToTable("OrderStatuses");
                entity.HasKey(e => e.StatusId);
            });

            // 17. Orders
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.OrderCode).HasComputedColumnSql("(N'MR-' + RIGHT(N'000000' + CAST(OrderId AS NVARCHAR(10)), 6))", stored: true);
                entity.HasIndex(e => e.OrderCode).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });
                entity.HasIndex(e => new { e.Status, e.CreatedAt });

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.HandledByUser)
                      .WithMany()
                      .HasForeignKey(e => e.HandledBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Coupon)
                      .WithMany()
                      .HasForeignKey(e => e.CouponId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.OrderStatus)
                      .WithMany()
                      .HasForeignKey(e => e.Status)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 18. OrderItems
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(e => e.OrderItemId);
                entity.Property(e => e.LineTotal).HasComputedColumnSql("((FramePrice + LensPrice) * Quantity)", stored: true);
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.ProductId);

                entity.HasOne(e => e.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Variant)
                      .WithMany()
                      .HasForeignKey(e => e.VariantId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.LensOption)
                      .WithMany()
                      .HasForeignKey(e => e.LensOptionId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // 19. Payments
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payments");
                entity.HasKey(e => e.PaymentId);
                entity.HasIndex(e => e.TransactionRef).IsUnique();
                entity.HasIndex(e => e.OrderId);

                entity.HasOne(e => e.Order)
                      .WithMany(o => o.Payments)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 20. OrderStatusHistory
            modelBuilder.Entity<OrderStatusHistory>(entity =>
            {
                entity.ToTable("OrderStatusHistory");
                entity.HasKey(e => e.HistoryId);
                entity.HasIndex(e => new { e.OrderId, e.ChangedAt });

                entity.HasOne(e => e.Order)
                      .WithMany(o => o.StatusHistories)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.OldOrderStatus)
                      .WithMany()
                      .HasForeignKey(e => e.OldStatus)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.NewOrderStatus)
                      .WithMany()
                      .HasForeignKey(e => e.NewStatus)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ChangedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ChangedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 21. Reviews
            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("Reviews");
                entity.HasKey(e => e.ReviewId);
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
                entity.HasIndex(e => new { e.ProductId, e.Status });

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Reviews)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ModeratedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ModeratedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 22. Database Views (vw_DailyRevenue, vw_BestSellers, vw_LowStock)
            modelBuilder.Entity<DailyRevenueReport>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_DailyRevenue");
            });

            modelBuilder.Entity<BestSellerReport>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_BestSellers");
            });

            modelBuilder.Entity<LowStockReport>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_LowStock");
            });
        }
    }
}

