using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


namespace CartService.Domain.Entity
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<CartStatus>();
            modelBuilder.HasPostgresEnum<OrderStatus>();
            var converterCartStatus = new EnumToStringConverter<CartStatus>();
            var converterOrderStatus = new EnumToStringConverter<OrderStatus>();

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("uuid_generate_v4()");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("carts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("uuid_generate_v4()");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("NOW()");
                entity.Property(e => e.Status).HasConversion(converterCartStatus)
                    // .HasColumnType("cart_status")
                    // .HasDefaultValue(CartStatus.Open)
                    ;
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("cart_items");
                entity.HasKey(e => new { e.CartId, e.ProductId });
                entity.Property(e => e.Count)
                    .IsRequired();
                entity.Property(e => e.Price)
                    .IsRequired();
                entity.HasOne(e => e.Cart)
                    .WithMany(c => c.CartItems)
                    .HasForeignKey(e => e.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("uuid_generate_v4()");
                entity.Property(e => e.Status).HasConversion(converterOrderStatus)
                    // .HasDefaultValue(OrderStatus.Open)
                    ;
                entity.Property(e => e.Total)
                    .IsRequired();
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Cart)
                    .WithMany()
                    .HasForeignKey(e => e.CartId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasDefaultValueSql("uuid_generate_v4()");
                entity.Property(e => e.Title)
                    .IsRequired();
                entity.Property(e => e.Description)
                    .IsRequired();
                entity.Property(e => e.Price)
                    .IsRequired();
                entity.Property(e => e.Photo);
            });
        }
    }
}
