using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class StoreContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillItem> BillItems => Set<BillItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Address
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstLine).IsRequired();
            entity.Property(e => e.City).IsRequired();
            entity.Property(e => e.State).IsRequired();
            entity.Property(e => e.PostalCode).IsRequired();
            entity.Property(e => e.Country).IsRequired();
        });

        // Shop
        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.VatNumber).HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.PromoStrategy).HasConversion<string>();

            entity.HasOne(s => s.Owner)
                  .WithMany(u => u.Shops)
                  .HasForeignKey(s => s.OwnerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Address)
                  .WithMany()
                  .HasForeignKey(s => s.AddressId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Schedule
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Day).HasConversion<string>();

            entity.HasOne(s => s.Shop)
                  .WithMany(sh => sh.DaysSchedule)
                  .HasForeignKey(s => s.ShopId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SellPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.BuyPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.UnitOfPrice).HasMaxLength(20);
            entity.Property(e => e.UnitOfVolume).HasMaxLength(20);
            entity.Property(e => e.UnitOfWeight).HasMaxLength(20);
            entity.Property(e => e.CodeBar).HasMaxLength(50);

            entity.HasOne(p => p.Shop)
                  .WithMany(s => s.Products)
                  .HasForeignKey(p => p.ShopId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Address)
                  .WithMany()
                  .HasForeignKey(p => p.AddressId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Promotion
        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(5,2)");

            entity.HasOne(p => p.Shop)
                .WithMany(s => s.Promotions)
                .HasForeignKey(p => p.ShopId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.Products)
                .WithMany(pr => pr.Promotions)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductPromotion",
                    j => j.HasOne<Product>()
                            .WithMany()
                            .HasForeignKey("ProductsId")
                            .OnDelete(DeleteBehavior.Restrict),
                    j => j.HasOne<Promotion>()
                            .WithMany()
                            .HasForeignKey("PromotionsId")
                            .OnDelete(DeleteBehavior.Cascade));
        });

        // PriceHistory
        modelBuilder.Entity<PriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SellPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.BuyPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.UnitOfPrice).HasMaxLength(20);
            entity.Property(e => e.ChangeReason).HasMaxLength(255);

            entity.HasOne(ph => ph.Product)
                  .WithMany(p => p.PriceHistories)
                  .HasForeignKey(ph => ph.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ph => new { ph.ProductId, ph.ChangedAt });
        });

        // Bill
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BillNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>();

            entity.Ignore(e => e.TotalAmount);
            entity.Ignore(e => e.TotalDiscount);
            entity.Ignore(e => e.TotalBeforeDiscount);
            entity.Ignore(e => e.AppliedPromotions);

            entity.HasOne(b => b.Shop)
                  .WithMany(s => s.Bills)
                  .HasForeignKey(b => b.ShopId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // BillItem
        modelBuilder.Entity<BillItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.FinalPrice).HasColumnType("decimal(18,2)");

            entity.Ignore(e => e.Discount);

            entity.HasOne(bi => bi.Bill)
                  .WithMany(b => b.BillItems)
                  .HasForeignKey(bi => bi.BillId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(bi => bi.Product)
                  .WithMany()
                  .HasForeignKey(bi => bi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(bi => bi.Promotion)
                  .WithMany()
                  .HasForeignKey(bi => bi.PromotionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ShopRating
        modelBuilder.Entity<ShopRating>()
            .HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ShopRating>()
            .HasOne(r => r.Shop)
            .WithMany(s => s.Ratings)
            .HasForeignKey(r => r.ShopId)
            .OnDelete(DeleteBehavior.NoAction); // ← coupe le cycle

        // Picture — tout en NoAction, géré dans SaveChangesAsync
        modelBuilder.Entity<Picture>()
            .HasOne(p => p.Shop)
            .WithMany(s => s.Pictures)
            .HasForeignKey(p => p.ShopId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Picture>()
            .HasOne(p => p.Product)
            .WithMany(p => p.Pictures)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Picture>()
            .HasOne(p => p.ShopRating)
            .WithMany(r => r.PicturesUrl)
            .HasForeignKey(p => p.ShopRatingId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        await HandleCascadesAsync(ct);
        return await base.SaveChangesAsync(ct);
    }

    private async Task HandleCascadesAsync(CancellationToken ct)
    {
        // Shops supprimés → supprimer leurs ShopRatings + Pictures
        var deletedShopIds = ChangeTracker.Entries<Shop>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity.Id)
            .ToList();

        if (deletedShopIds.Any())
        {
            // Pictures du shop
            var shopPictures = await Set<Picture>()
                .Where(p => p.ShopId.HasValue && deletedShopIds.Contains(p.ShopId.Value))
                .ToListAsync(ct);
            Set<Picture>().RemoveRange(shopPictures);

            // ShopRatings du shop (avec leurs pictures)
            var ratings = await Set<ShopRating>()
                .Where(r => deletedShopIds.Contains(r.ShopId))
                .ToListAsync(ct);

            var ratingIds = ratings.Select(r => r.Id).ToList();
            if (ratingIds.Any())
            {
                var ratingPictures = await Set<Picture>()
                    .Where(p => p.ShopRatingId.HasValue && ratingIds.Contains(p.ShopRatingId.Value))
                    .ToListAsync(ct);
                Set<Picture>().RemoveRange(ratingPictures);
            }

            Set<ShopRating>().RemoveRange(ratings);
        }

        // ShopRatings supprimés directement → supprimer leurs Pictures
        var deletedRatingIds = ChangeTracker.Entries<ShopRating>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity.Id)
            .ToList();

        if (deletedRatingIds.Any())
        {
            var ratingPictures = await Set<Picture>()
                .Where(p => p.ShopRatingId.HasValue && deletedRatingIds.Contains(p.ShopRatingId.Value))
                .ToListAsync(ct);
            Set<Picture>().RemoveRange(ratingPictures);
        }

        // Products supprimés → supprimer leurs Pictures
        var deletedProductIds = ChangeTracker.Entries<Product>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity.Id)
            .ToList();

        if (deletedProductIds.Any())
        {
            var productPictures = await Set<Picture>()
                .Where(p => p.ProductId.HasValue && deletedProductIds.Contains(p.ProductId.Value))
                .ToListAsync(ct);
            Set<Picture>().RemoveRange(productPictures);
        }
    }
}