using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Inventory relationships
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasMany(i => i.Items)
                    .WithOne(item => item.Inventory)
                    .HasForeignKey(item => item.InventoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(i => i.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure Item relationships
            modelBuilder.Entity<Item>(entity =>
            {
                entity.Property(i => i.Price)
                    .HasColumnType("decimal(10,2)");

                entity.Property(i => i.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(i => new { i.InventoryId, i.CustomId })
                    .IsUnique()
                    .HasDatabaseName("IX_Item_InventoryId_CustomId");
            });
        }

        public override int SaveChanges()
        {
            SetTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Inventory || e.Entity is Item)
                .Where(e => e.State == EntityState.Added);

            foreach (var entry in entries)
            {
                if (entry.Entity is Inventory inventory)
                    inventory.CreatedAt = DateTime.UtcNow;
                else if (entry.Entity is Item item)
                    item.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}