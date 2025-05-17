using System;
using Microsoft.EntityFrameworkCore;

namespace masterPiece.Models
{
    public partial class MyDbContext : DbContext
    {
        public MyDbContext() { }

        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options) { }

        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<CartDetail> CartDetails { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<User> Users { get; set; }

        // ✅ FarmerWork مرتبط بجدول اسمه FarmerWork
        public virtual DbSet<FarmerWork> FarmerWorks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#warning Move connection string to configuration for security
            optionsBuilder.UseSqlServer("Server=DESKTOP-KCF2RKO;Database=localFarms;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.ToTable("Cart");
                entity.Property(e => e.UserID);
                entity.HasOne(d => d.User).WithMany(p => p.Carts)
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<CartDetail>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Quantity).HasDefaultValue(1);
                entity.HasOne(d => d.Cart).WithMany(p => p.CartDetails)
                    .HasForeignKey(d => d.CartID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                entity.HasOne(d => d.Product).WithMany(p => p.CartDetails)
                    .HasForeignKey(d => d.ProductID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.ImagePath).HasMaxLength(255);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.DeliveryAddress).HasMaxLength(255);
                entity.HasOne(d => d.User).WithMany(p => p.Orders)
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");
                entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
                entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.ProductID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.TransactionID).HasMaxLength(100);
                entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                    .HasForeignKey(d => d.OrderID)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnType("text");
                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.ImagePath).HasMaxLength(255);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasOne(d => d.Category).WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId);
                entity.HasOne(d => d.User).WithMany(p => p.Products)
                    .HasForeignKey(d => d.UserID);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Comment).HasColumnType("text");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.HasOne(d => d.Order).WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.OrderID);
                entity.HasOne(d => d.Product).WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.ProductID);
                entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.UserID);
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.PlanName).HasMaxLength(50);
                entity.Property(e => e.StartDate).HasColumnType("datetime");
                entity.Property(e => e.EndDate).HasColumnType("datetime");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                    .HasForeignKey(d => d.UserID);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Username).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.UserType).HasMaxLength(10);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // ✅ FarmerWork مربوط بجدول FarmerWork (اسم الجدول في SQL بدون s)
            modelBuilder.Entity<FarmerWork>(entity =>
            {
                entity.ToTable("FarmerWork"); // ⚠️ هذا هو التعديل الحاسم
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
                entity.Property(e => e.ImagePath).HasMaxLength(255);
                entity.Property(e => e.CreatedAt)
                      .HasColumnType("datetime")
                      .HasDefaultValueSql("(getdate())");

                entity.HasOne(e => e.User)
                      .WithMany(u => u.FarmerWorks)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_FarmerWork_Users");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
