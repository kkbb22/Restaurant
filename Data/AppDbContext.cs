using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            // هاد السطر بيحل مشكلة التوقيت (DateTime) في PostgreSQL فوراً
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        // جداول قاعدة البيانات
        public DbSet<RestaurantProfile> Restaurants { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<RestaurantTable> RestaurantTables { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<RestaurantSettings> RestaurantSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // إعدادات الجداول والأسماء
            modelBuilder.Entity<RestaurantTable>().ToTable("RestaurantTables");

            // علاقة الحجوزات مع الزبائن
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة الطلبات مع الزبائن
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ضبط دقة الأرقام العشرية (Decimal) عشان ما تضرب في الـ Database
            modelBuilder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");
        }
    }
}