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
        public virtual DbSet<Etag> Etags { get; set; } = null!;
        public virtual DbSet<EtagType> EtagTypes { get; set; } = null!;
        public virtual DbSet<House> Houses { get; set; } = null!;
        public virtual DbSet<MarketZone> MarketZones { get; set; } = null!;
        public virtual DbSet<Menu> Menus { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public virtual DbSet<Package> Packages { get; set; } = null!;
        public virtual DbSet<PackageETagTypeMapping> PackageETagTypeMappings { get; set; } = null!;
        public virtual DbSet<ProductCategory> ProductCategories { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Store> Stores { get; set; } = null!;
        public virtual DbSet<StoreService> StoreServices { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;
        public virtual DbSet<Wallet> Wallets { get; set; } = null!;
        public virtual DbSet<WalletType> WalletTypes { get; set; } = null!;
        public virtual DbSet<WalletTypeStoreServiceMapping> WalletTypeStoreServiceMappings { get; set; } = null!;
        public virtual DbSet<Zone> Zones { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=14.225.204.144,6789;Database=VegaCityApp;User Id=sa;Password=s@123456;Encrypt=True;TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Deposit>(entity =>
            {
                entity.ToTable("Deposit");

                entity.HasIndex(e => e.EtagId, "IX_Deposit_EtagId");

                entity.HasIndex(e => e.OrderId, "IX_Deposit_OrderId");

                entity.HasIndex(e => e.WalletId, "IX_Deposit_WalletId");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.PaymentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Etag)
                    .WithMany(p => p.Deposits)
                    .HasForeignKey(d => d.EtagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Deposit_ETag");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Deposits)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Deposit_Order");

                entity.HasOne(d => d.Wallet)
                    .WithMany(p => p.Deposits)
                    .HasForeignKey(d => d.WalletId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Deposit_Wallet");
            });

            modelBuilder.Entity<DisputeReport>(entity =>
            {
                entity.HasIndex(e => e.StoreId, "IX_DisputeReports_StoreId");

                entity.HasIndex(e => e.UserId, "IX_DisputeReports_UserId");

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
                    .HasConstraintName("FK_DisputeReports_Store");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.DisputeReports)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_DisputeReports_User");
            });

            modelBuilder.Entity<Etag>(entity =>
            {
                entity.ToTable("ETag");

                entity.HasIndex(e => e.EtagTypeId, "IX_ETag_ETagTypeId");

                entity.HasIndex(e => e.MarketZoneId, "IX_ETag_MarketZoneId");

                entity.HasIndex(e => e.WalletId, "IX_ETag_WalletId");

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

                entity.Property(e => e.EndDate).HasColumnType("datetime");

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

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.EtagType)
                    .WithMany(p => p.Etags)
                    .HasForeignKey(d => d.EtagTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ETag_ETagType");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.Etags)
                    .HasForeignKey(d => d.MarketZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ETag_MarketZone");

                entity.HasOne(d => d.Wallet)
                    .WithMany(p => p.Etags)
                    .HasForeignKey(d => d.WalletId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ETag_Wallet");
            });

            modelBuilder.Entity<EtagType>(entity =>
            {
                entity.ToTable("ETagType");

                entity.HasIndex(e => e.MarketZoneId, "IX_ETagType_MarketZoneId");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.BonusRate).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.EtagTypes)
                    .HasForeignKey(d => d.MarketZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ETagType_MarketZone");

                entity.HasOne(d => d.WalletType)
                    .WithMany(p => p.EtagTypes)
                    .HasForeignKey(d => d.WalletTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ETagType_WalletType");
            });

            modelBuilder.Entity<House>(entity =>
            {
                entity.ToTable("House");

                entity.HasIndex(e => e.ZoneId, "IX_House_ZoneId");

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

                entity.HasIndex(e => e.StoreId, "IX_Menu_StoreId");

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
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Menu_Store");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.HasIndex(e => e.EtagId, "IX_Order_ETagId");

                entity.HasIndex(e => e.StoreId, "IX_Order_StoreId");

                entity.HasIndex(e => e.UserId, "IX_Order_UserId");

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

                entity.Property(e => e.SaleType).HasMaxLength(50);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

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

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");

                entity.HasIndex(e => e.MenuId, "IX_OrderDetail_MenuId");

                entity.HasIndex(e => e.OrderId, "IX_OrderDetail_OrderId");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

                entity.Property(e => e.Vatrate).HasColumnName("VATRate");

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("FK_OrderDetail_Menu");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_OrderDetail_Order");
            });

            modelBuilder.Entity<Package>(entity =>
            {
                entity.ToTable("Package");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.EndDate)
                    .HasColumnType("datetime")
                    .HasColumnName("endDate");

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.StartDate)
                    .HasColumnType("datetime")
                    .HasColumnName("startDate");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<PackageETagTypeMapping>(entity =>
            {
                entity.ToTable("PackageE-TagTypeMapping");

                entity.HasIndex(e => e.EtagTypeId, "IX_PackageE-TagTypeMapping_ETagTypeId");

                entity.HasIndex(e => e.PackageId, "IX_PackageE-TagTypeMapping_PackageId");

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
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PackageE-TagTypeMapping_ETagType");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.PackageETagTypeMappings)
                    .HasForeignKey(d => d.PackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PackageE-TagTypeMapping_package");
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("ProductCategory");

                entity.HasIndex(e => e.MenuId, "IX_ProductCategory_MenuId");

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

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.ProductCategories)
                    .HasForeignKey(d => d.MenuId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductCategory_Menu");
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

                entity.HasIndex(e => e.HouseId, "IX_Store_HouseId");

                entity.HasIndex(e => e.MarketZoneId, "IX_Store_MarketZoneId");

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

            modelBuilder.Entity<StoreService>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.StoreServices)
                    .HasForeignKey(d => d.StoreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StoreServices_Store");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transaction");

                entity.HasIndex(e => e.StoreId, "IX_Transaction_StoreId");

                entity.HasIndex(e => e.WalletId, "IX_Transaction_WalletId");

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

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_Transaction_Store");

                entity.HasOne(d => d.Wallet)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.WalletId)
                    .HasConstraintName("FK_Transaction_Wallet");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.RoleId, "IX_User_RoleId");

                entity.HasIndex(e => e.StoreId, "IX_User_StoreId");

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

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Role");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_User_Store1");
            });

            modelBuilder.Entity<UserRefreshToken>(entity =>
            {
                entity.ToTable("UserRefreshToken");

                entity.HasIndex(e => e.UserId, "IX_UserRefreshToken_UserId");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Token).IsUnicode(false);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRefreshTokens)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRefreshToken_User");
            });

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.ToTable("Wallet");

                entity.HasIndex(e => e.StoreId, "IX_Wallet_StoreId");

                entity.HasIndex(e => e.UserId, "IX_Wallet_UserId");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ExpireDate).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Wallets)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_Wallet_Store");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Wallets)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Wallet_User");
            });

            modelBuilder.Entity<WalletType>(entity =>
            {
                entity.ToTable("WalletType");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.WalletTypes)
                    .HasForeignKey(d => d.MarketZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WalletType_MarketZone");
            });

            modelBuilder.Entity<WalletTypeStoreServiceMapping>(entity =>
            {
                entity.ToTable("WalletTypeStoreServiceMapping");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

                entity.HasOne(d => d.StoreService)
                    .WithMany(p => p.WalletTypeStoreServiceMappings)
                    .HasForeignKey(d => d.StoreServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WalletTypeStoreServiceMapping_StoreServices");

                entity.HasOne(d => d.WalletType)
                    .WithMany(p => p.WalletTypeStoreServiceMappings)
                    .HasForeignKey(d => d.WalletTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WalletTypeStoreServiceMapping_WalletType");
            });

            modelBuilder.Entity<Zone>(entity =>
            {
                entity.ToTable("Zone");

                entity.HasIndex(e => e.MarketZoneId, "IX_Zone_MarketZoneId");

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
