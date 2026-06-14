using Microsoft.EntityFrameworkCore;
using MiniShop.ApplicationCore.Entities;

namespace MiniShop.Infrastructure.Data;

public class MiniShopDbContext : DbContext
{
    public MiniShopDbContext(DbContextOptions<MiniShopDbContext> options) : base(options)
    {
        
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(builder =>
        {
            builder.HasMany(o => o.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(o => o.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.Property(x => x.Type)
                .HasMaxLength(500);

            builder.Property(x => x.Content)
                .HasMaxLength(4000);
        });
        
        modelBuilder.Entity<InboxMessage>(builder =>
        {
            builder.HasIndex(x => new { x.EventId, x.Consumer })
                .IsUnique();

            builder.Property(x => x.Type)
                .HasMaxLength(500);

            builder.Property(x => x.Consumer)
                .HasMaxLength(200);

            builder.Property(x => x.Content)
                .HasMaxLength(4000);

            builder.Property(x => x.Error)
                .HasMaxLength(4000);
        });
        
        modelBuilder.Entity<Product>()
            .Property(product => product.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(orderItem => orderItem.UnitPrice)
            .HasPrecision(18, 2);
    }
}