using Microsoft.EntityFrameworkCore;
using ProductionCaptchaSystem.Entities;

namespace ProductionCaptchaSystem.Infrastructure.Persistence;

public class CaptchaDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Counterparty> Counterparties { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemType> ItemTypes { get; set; }
    public DbSet<Specification> Specifications { get; set; }
    public DbSet<SpecificationLine> SpecificationLines { get; set; }
    public DbSet<CustomerOrder> CustomerOrders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<ProductionOrder> ProductionOrders { get; set; }
    public DbSet<ProductionOrderProduct> ProductionOrderProducts { get; set; }
    public DbSet<ProductionOrderMaterial> ProductionOrderMaterials { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder
                .UseNpgsql("Host=localhost;Username=postgres;Password=111;Database=importdb")
                .UseLazyLoadingProxies();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Username).HasColumnName("Username").HasMaxLength(50);
            entity.Property(e => e.Password).HasColumnName("Password").HasMaxLength(255);
            entity.Property(e => e.IsAdmin).HasColumnName("IsAdmin");
            entity.Property(e => e.IsBlocked).HasColumnName("IsBlocked");
            entity.Property(e => e.BlockedUntil).HasColumnName("BlockedUntil");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
        });

        modelBuilder.Entity<Counterparty>(entity =>
        {
            entity.ToTable("Counterparty");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Name).HasColumnName("Name").HasMaxLength(255);
            entity.Property(e => e.Inn).HasColumnName("Inn").HasMaxLength(12);
            entity.Property(e => e.Address).HasColumnName("Address");
            entity.Property(e => e.Phone).HasColumnName("Phone").HasMaxLength(50);
            entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
        });

        modelBuilder.Entity<ItemType>(entity =>
        {
            entity.ToTable("ItemType");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Name).HasColumnName("Name").HasMaxLength(50);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Item");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Name).HasColumnName("Name").HasMaxLength(255);
            entity.Property(e => e.Article).HasColumnName("Article").HasMaxLength(100);
            entity.Property(e => e.Unit).HasColumnName("Unit").HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnName("Price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.TypeId).HasColumnName("TypeId");
            entity.Property(e => e.Description).HasColumnName("Description");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");

            entity.HasOne(d => d.Type)
                .WithMany()
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("FK_Item_ItemType");
        });

        modelBuilder.Entity<Specification>(entity =>
        {
            entity.ToTable("Specification");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.ProductId).HasColumnName("ProductId");
            entity.Property(e => e.Name).HasColumnName("Name").HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("Description");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");

            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Specification_Product");
        });

        modelBuilder.Entity<SpecificationLine>(entity =>
        {
            entity.ToTable("SpecificationLine");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.SpecificationId).HasColumnName("SpecificationId");
            entity.Property(e => e.MaterialId).HasColumnName("MaterialId");
            entity.Property(e => e.Quantity).HasColumnName("Quantity").HasColumnType("decimal(18,3)");

            entity.HasOne(d => d.Specification)
                .WithMany()
                .HasForeignKey(d => d.SpecificationId)
                .HasConstraintName("FK_SpecificationLine_Specification");

            entity.HasOne(d => d.Material)
                .WithMany()
                .HasForeignKey(d => d.MaterialId)
                .HasConstraintName("FK_SpecificationLine_Material");
        });

        modelBuilder.Entity<CustomerOrder>(entity =>
        {
            entity.ToTable("CustomerOrder");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.OrderNumber).HasColumnName("OrderNumber").HasMaxLength(50);
            entity.Property(e => e.CounterpartyId).HasColumnName("CounterpartyId");
            entity.Property(e => e.OrderDate).HasColumnName("OrderDate");
            entity.Property(e => e.RequiredDate).HasColumnName("RequiredDate");
            entity.Property(e => e.Status).HasColumnName("Status").HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnName("TotalAmount").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Comment).HasColumnName("Comment");

            entity.HasOne(d => d.Counterparty)
                .WithMany()
                .HasForeignKey(d => d.CounterpartyId)
                .HasConstraintName("FK_CustomerOrder_Counterparty");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItem");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.CustomerOrderId).HasColumnName("CustomerOrderId");
            entity.Property(e => e.ProductId).HasColumnName("ProductId");
            entity.Property(e => e.Quantity).HasColumnName("Quantity").HasColumnType("decimal(18,3)");
            entity.Property(e => e.Price).HasColumnName("Price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Amount).HasColumnName("Amount").HasColumnType("decimal(18,2)");

            entity.HasOne(d => d.CustomerOrder)
                .WithMany()
                .HasForeignKey(d => d.CustomerOrderId)
                .HasConstraintName("FK_OrderItem_CustomerOrder");

            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_OrderItem_Product");
        });

        modelBuilder.Entity<ProductionOrder>(entity =>
        {
            entity.ToTable("ProductionOrder");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.OrderNumber).HasColumnName("OrderNumber").HasMaxLength(50);
            entity.Property(e => e.CustomerOrderId).HasColumnName("CustomerOrderId");
            entity.Property(e => e.StartDate).HasColumnName("StartDate");
            entity.Property(e => e.PlannedEndDate).HasColumnName("PlannedEndDate");
            entity.Property(e => e.ActualEndDate).HasColumnName("ActualEndDate");
            entity.Property(e => e.Status).HasColumnName("Status").HasMaxLength(50);
            entity.Property(e => e.Comment).HasColumnName("Comment");

            entity.HasOne(d => d.CustomerOrder)
                .WithMany()
                .HasForeignKey(d => d.CustomerOrderId)
                .HasConstraintName("FK_ProductionOrder_CustomerOrder");
        });

        modelBuilder.Entity<ProductionOrderProduct>(entity =>
        {
            entity.ToTable("ProductionOrderProduct");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.ProductionOrderId).HasColumnName("ProductionOrderId");
            entity.Property(e => e.ProductId).HasColumnName("ProductId");
            entity.Property(e => e.Quantity).HasColumnName("Quantity").HasColumnType("decimal(18,3)");
            entity.Property(e => e.ProducedQuantity).HasColumnName("ProducedQuantity").HasColumnType("decimal(18,3)");

            entity.HasOne(d => d.ProductionOrder)
                .WithMany()
                .HasForeignKey(d => d.ProductionOrderId)
                .HasConstraintName("FK_ProductionOrderProduct_ProductionOrder");

            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ProductionOrderProduct_Product");
        });

        modelBuilder.Entity<ProductionOrderMaterial>(entity =>
        {
            entity.ToTable("ProductionOrderMaterial");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.ProductionOrderId).HasColumnName("ProductionOrderId");
            entity.Property(e => e.MaterialId).HasColumnName("MaterialId");
            entity.Property(e => e.RequiredQuantity).HasColumnName("RequiredQuantity").HasColumnType("decimal(18,3)");
            entity.Property(e => e.UsedQuantity).HasColumnName("UsedQuantity").HasColumnType("decimal(18,3)");

            entity.HasOne(d => d.ProductionOrder)
                .WithMany()
                .HasForeignKey(d => d.ProductionOrderId)
                .HasConstraintName("FK_ProductionOrderMaterial_ProductionOrder");

            entity.HasOne(d => d.Material)
                .WithMany()
                .HasForeignKey(d => d.MaterialId)
                .HasConstraintName("FK_ProductionOrderMaterial_Material");
        });

        base.OnModelCreating(modelBuilder);
    }
}
