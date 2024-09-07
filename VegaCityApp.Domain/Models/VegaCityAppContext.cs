using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VegaCityApp.Domain.Models
{
    public partial class VegaCityAppContext : DbContext
    {
        public VegaCityAppContext()
        {
        }

        public VegaCityAppContext(DbContextOptions<VegaCityAppContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Deposit> Deposits { get; set; } = null!;
        public virtual DbSet<DisputeReport> DisputeReports { get; set; } = null!;
        public virtual DbSet<ENotification> ENotifications { get; set; } = null!;
        public virtual DbSet<Etag> Etags { get; set; } = null!;
        public virtual DbSet<EtagType> EtagTypes { get; set; } = null!;
        public virtual DbSet<House> Houses { get; set; } = null!;
        public virtual DbSet<MarketZone> MarketZones { get; set; } = null!;
        public virtual DbSet<Menu> Menus { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<Package> Packages { get; set; } = null!;
        public virtual DbSet<PackageETagTypeMapping> PackageETagTypeMappings { get; set; } = null!;
        public virtual DbSet<ProductCategory> ProductCategories { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Store> Stores { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserWallet> UserWallets { get; set; } = null!;
        public virtual DbSet<Zone> Zones { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=14.225.204.144;Database=VegaCityApp;User Id=vegadb;Password=vega12345;Encrypt=True;TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Deposit>(entity =>
            {
                entity.ToTable("Deposit");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FullName).HasMaxLength(50);

                entity.Property(e => e.IsIncrease)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("isIncrease");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.UserWallet)
                    .WithMany(p => p.Deposits)
                    .HasForeignKey(d => d.UserWalletId)
                    .HasConstraintName("FK_Deposit_UserWallet");
            });

            modelBuilder.Entity<DisputeReport>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.IssueType)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Resolution).IsUnicode(false);

                entity.Property(e => e.ResolvedBy)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ResolvedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.DisputeReports)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK__DisputeRe__Store__151B244E");

                entity.HasOne(d => d.Transaction)
                    .WithMany(p => p.DisputeReports)
                    .HasForeignKey(d => d.TransactionId)
                    .HasConstraintName("FK__DisputeRe__Trans__160F4887");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.DisputeReports)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__DisputeRe__UserI__17036CC0");
            });

            modelBuilder.Entity<ENotification>(entity =>
            {
                entity.ToTable("E_Notification");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EtagId).HasColumnName("ETagId");

                entity.Property(e => e.ExpireDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.Etag)
                    .WithMany(p => p.ENotifications)
                    .HasForeignKey(d => d.EtagId)
                    .HasConstraintName("FK__E_Notific__ETagI__17F790F9");
            });

            modelBuilder.Entity<Etag>(entity =>
            {
                entity.ToTable("ETag");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Birthday).HasColumnType("date");

                entity.Property(e => e.Cccd)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .HasColumnName("CCCD")
                    .IsFixedLength();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EtagCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EtagTypeId).HasColumnName("ETagTypeId");

                entity.Property(e => e.FullName).HasMaxLength(50);

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Qrcode)
                    .IsUnicode(false)
                    .HasColumnName("QRCode");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.EtagType)
                    .WithMany(p => p.Etags)
                    .HasForeignKey(d => d.EtagTypeId)
                    .HasConstraintName("FK_ETag_ETagType");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.Etags)
                    .HasForeignKey(d => d.MarketZoneId)
                    .HasConstraintName("FK_ETag_MarketZone");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Etags)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_ETag_User");

                entity.HasOne(d => d.UserWallet)
                    .WithMany(p => p.Etags)
                    .HasForeignKey(d => d.UserWalletId)
                    .HasConstraintName("FK_ETag_UserWallet");
            });

            modelBuilder.Entity<EtagType>(entity =>
            {
                entity.ToTable("ETagType");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.BonusRate).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.EtagTypes)
                    .HasForeignKey(d => d.MarketZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ETagType_MarketZone");
            });

            modelBuilder.Entity<House>(entity =>
            {
                entity.ToTable("House");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address).HasMaxLength(50);

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.HouseName).HasMaxLength(50);

                entity.Property(e => e.Location).HasMaxLength(50);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Zone)
                    .WithMany(p => p.Houses)
                    .HasForeignKey(d => d.ZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_House_Zone");
            });

            modelBuilder.Entity<MarketZone>(entity =>
            {
                entity.ToTable("MarketZone");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Location).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ShortName).HasMaxLength(50);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("Menu");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.MenuJson).IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Menus)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_Menu_Store");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EtagId).HasColumnName("ETagId");

                entity.Property(e => e.InvoiceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.PaymentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProductJson).IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Vatrate).HasColumnName("VATRate");

                entity.HasOne(d => d.Etag)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.EtagId)
                    .HasConstraintName("FK_Order_ETag");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_Order_Store");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Order_User");
            });

            modelBuilder.Entity<Package>(entity =>
            {
                entity.ToTable("Package");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.EndDate)
                    .HasColumnType("datetime")
                    .HasColumnName("endDate");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.StartDate)
                    .HasColumnType("datetime")
                    .HasColumnName("startDate");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.Packages)
                    .HasForeignKey(d => d.MarketZoneId)
                    .HasConstraintName("FK_Package_MarketZone");
            });

            modelBuilder.Entity<PackageETagTypeMapping>(entity =>
            {
                entity.ToTable("PackageE-TagTypeMapping");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EtagTypeId).HasColumnName("ETagTypeId");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.EtagType)
                    .WithMany(p => p.PackageETagTypeMappings)
                    .HasForeignKey(d => d.EtagTypeId)
                    .HasConstraintName("FK_PackageE-TagTypeMapping_ETagType");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.PackageETagTypeMappings)
                    .HasForeignKey(d => d.PackageId)
                    .HasConstraintName("FK_PackageE-TagTypeMapping_package");
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("ProductCategory");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.ProductCategories)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_ProductCategory_Store");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Store>(entity =>
            {
                entity.ToTable("Store");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ShortName).HasMaxLength(50);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.House)
                    .WithMany(p => p.Stores)
                    .HasForeignKey(d => d.HouseId)
                    .HasConstraintName("FK_Store_House");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.Stores)
                    .HasForeignKey(d => d.MarketZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Store_MarketZone");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transaction");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Currency)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.EtagId).HasColumnName("ETagId");

                entity.Property(e => e.MarketZoneEtagId).HasColumnName("MarketZoneETagId");

                entity.Property(e => e.PaymentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Etag)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.EtagId)
                    .HasConstraintName("FK_Transaction_ETag");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_Transaction_Order");

                entity.HasOne(d => d.UserWallet)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.UserWalletId)
                    .HasConstraintName("FK_Transaction_Deposit");

                entity.HasOne(d => d.UserWalletNavigation)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.UserWalletId)
                    .HasConstraintName("FK_Transaction_UserWallet");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address).HasMaxLength(200);

                entity.Property(e => e.Birthday).HasColumnType("date");

                entity.Property(e => e.Cccd)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .HasColumnName("CCCD")
                    .IsFixedLength();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description)
                    .HasMaxLength(400)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FullName).HasMaxLength(50);

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.PinCode).IsUnicode(false);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_User_Role");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_User_Store");
            });

            modelBuilder.Entity<UserWallet>(entity =>
            {
                entity.ToTable("UserWallet");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserWallets)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_UserWallet_User");
            });

            modelBuilder.Entity<Zone>(entity =>
            {
                entity.ToTable("Zone");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Location).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.Zones)
                    .HasForeignKey(d => d.MarketZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Zone_MarketZone");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
