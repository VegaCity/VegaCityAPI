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

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<MarketZone> MarketZones { get; set; } = null!;
        public virtual DbSet<MarketZoneCard> MarketZoneCards { get; set; } = null!;
        public virtual DbSet<MarketZoneCardType> MarketZoneCardTypes { get; set; } = null!;
        public virtual DbSet<Menu> Menus { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public virtual DbSet<OwnerStore> OwnerStores { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ProductCategory> ProductCategories { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Store> Stores { get; set; } = null!;
        public virtual DbSet<StoreSession> StoreSessions { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserAction> UserActions { get; set; } = null!;
        public virtual DbSet<UserActionType> UserActionTypes { get; set; } = null!;
        public virtual DbSet<UserWallet> UserWallets { get; set; } = null!;
        public virtual DbSet<WalletType> WalletTypes { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=LAPTOP-R0K7KBGI\\TRANGQUOCDAT;Database=VegaCityApp;User Id=sa;Password=12345;Encrypt=True;TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_Account_Role");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Account_User");
            });

            modelBuilder.Entity<MarketZone>(entity =>
            {
                entity.ToTable("MarketZone");

                entity.Property(e => e.Id).ValueGeneratedNever();

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

            modelBuilder.Entity<MarketZoneCard>(entity =>
            {
                entity.ToTable("MarketZoneCard");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Birthday).HasColumnType("date");

                entity.Property(e => e.Cccd)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .HasColumnName("CCCD")
                    .IsFixedLength();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

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
                    .HasColumnName("upsDate");

                entity.HasOne(d => d.MarketZoneCardType)
                    .WithMany(p => p.MarketZoneCards)
                    .HasForeignKey(d => d.MarketZoneCardTypeId)
                    .HasConstraintName("FK_MarketZoneCard_MarketZoneCardType");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.MarketZoneCards)
                    .HasForeignKey(d => d.MarketZoneId)
                    .HasConstraintName("FK_MarketZoneCard_MarketZone");
            });

            modelBuilder.Entity<MarketZoneCardType>(entity =>
            {
                entity.ToTable("MarketZoneCardType");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.MarketZoneCardTypes)
                    .HasForeignKey(d => d.MarketZoneId)
                    .HasConstraintName("FK_MarketZoneCardType_MarketZone");
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("Menu");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.ImageUrl).IsUnicode(false);

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

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.InvoiceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.PaymentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

                entity.Property(e => e.Vatrate).HasColumnName("VATRate");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InvoiceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Vatamount).HasColumnName("VATAmount");

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("FK_OrderDetail_Menu");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_OrderDetail_Order");
            });

            modelBuilder.Entity<OwnerStore>(entity =>
            {
                entity.ToTable("OwnerStore");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.OwnerName).HasMaxLength(50);

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.OwnerStores)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_OwnerStore_Store");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.OwnerStores)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_OwnerStore_User");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Size).HasMaxLength(20);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

                entity.HasOne(d => d.ProductCategory)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductCategoryId)
                    .HasConstraintName("FK_Product_ProductCategory");
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("ProductCategory");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.ProductCategories)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("FK_ProductCategory_Menu");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Store>(entity =>
            {
                entity.ToTable("Store");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ShortName).HasMaxLength(50);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.Stores)
                    .HasForeignKey(d => d.MarketZoneId)
                    .HasConstraintName("FK_Store_MarketZone");
            });

            modelBuilder.Entity<StoreSession>(entity =>
            {
                entity.ToTable("StoreSession");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.EDate)
                    .HasColumnType("datetime")
                    .HasColumnName("eDate");

                entity.Property(e => e.StDate)
                    .HasColumnType("datetime")
                    .HasColumnName("stDate");

                entity.Property(e => e.TotalAmount).HasColumnName("totalAmount");

                entity.Property(e => e.TotalProduct).HasColumnName("totalProduct");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.StoreSessions)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_StoreSession_Store");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transaction");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.Currency)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.PaymentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.MarketZoneCard)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.MarketZoneCardId)
                    .HasConstraintName("FK_Transaction_MarketZoneCard");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.MarketZoneId)
                    .HasConstraintName("FK_Transaction_MarketZone");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_Transaction_Order");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_Transaction_Store");

                entity.HasOne(d => d.UserWallet)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.UserWalletId)
                    .HasConstraintName("FK_Transaction_UserWallet");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.Cccd, "IX_User")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Birthday).HasColumnType("date");

                entity.Property(e => e.Cccd)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .HasColumnName("CCCD")
                    .IsFixedLength();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FullName).HasMaxLength(50);

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.PinCode)
                    .IsUnicode(false)
                    .HasColumnName("pinCode");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");
            });

            modelBuilder.Entity<UserAction>(entity =>
            {
                entity.ToTable("UserAction");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

                entity.HasOne(d => d.MarketZoneCard)
                    .WithMany(p => p.UserActions)
                    .HasForeignKey(d => d.MarketZoneCardId)
                    .HasConstraintName("FK_UserAction_MarketZoneCard");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.UserActions)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_UserAction_Order");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.UserActions)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_UserAction_Store");

                entity.HasOne(d => d.UserActionType)
                    .WithMany(p => p.UserActions)
                    .HasForeignKey(d => d.UserActionTypeId)
                    .HasConstraintName("FK_UserAction_UserActionType");

                entity.HasOne(d => d.UserWallet)
                    .WithMany(p => p.UserActions)
                    .HasForeignKey(d => d.UserWalletId)
                    .HasConstraintName("FK_UserAction_UserWallet");
            });

            modelBuilder.Entity<UserActionType>(entity =>
            {
                entity.ToTable("UserActionType");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.UserActionTypes)
                    .HasForeignKey(d => d.MarketZoneId)
                    .HasConstraintName("FK_UserActionType_MarketZone");
            });

            modelBuilder.Entity<UserWallet>(entity =>
            {
                entity.ToTable("UserWallet");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserWallets)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_UserWallet_User");

                entity.HasOne(d => d.WalletType)
                    .WithMany(p => p.UserWallets)
                    .HasForeignKey(d => d.WalletTypeId)
                    .HasConstraintName("FK_UserWallet_WalletType");
            });

            modelBuilder.Entity<WalletType>(entity =>
            {
                entity.ToTable("WalletType");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.BonusRate)
                    .HasColumnType("decimal(2, 2)")
                    .HasColumnName("bonusRate");

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
                    .HasConstraintName("FK_WalletType_MarketZone");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
