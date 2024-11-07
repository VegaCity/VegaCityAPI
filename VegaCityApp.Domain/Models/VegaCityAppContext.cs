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

        public virtual DbSet<AggregatedCounter> AggregatedCounters { get; set; } = null!;
        public virtual DbSet<Counter> Counters { get; set; } = null!;
        public virtual DbSet<CustomerMoneyTransfer> CustomerMoneyTransfers { get; set; } = null!;
        public virtual DbSet<Deposit> Deposits { get; set; } = null!;
        public virtual DbSet<Hash> Hashes { get; set; } = null!;
        public virtual DbSet<IssueType> IssueTypes { get; set; } = null!;
        public virtual DbSet<Job> Jobs { get; set; } = null!;
        public virtual DbSet<JobParameter> JobParameters { get; set; } = null!;
        public virtual DbSet<JobQueue> JobQueues { get; set; } = null!;
        public virtual DbSet<List> Lists { get; set; } = null!;
        public virtual DbSet<MarketZone> MarketZones { get; set; } = null!;
        public virtual DbSet<MarketZoneConfig> MarketZoneConfigs { get; set; } = null!;
        public virtual DbSet<Menu> Menus { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public virtual DbSet<Package> Packages { get; set; } = null!;
        public virtual DbSet<PackageDetail> PackageDetails { get; set; } = null!;
        public virtual DbSet<PackageItem> PackageItems { get; set; } = null!;
        public virtual DbSet<PackageOrder> PackageOrders { get; set; } = null!;
        public virtual DbSet<PackageType> PackageTypes { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ProductCategory> ProductCategories { get; set; } = null!;
        public virtual DbSet<Promotion> Promotions { get; set; } = null!;
        public virtual DbSet<PromotionOrder> PromotionOrders { get; set; } = null!;
        public virtual DbSet<Report> Reports { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Schema> Schemas { get; set; } = null!;
        public virtual DbSet<Server> Servers { get; set; } = null!;
        public virtual DbSet<Set> Sets { get; set; } = null!;
        public virtual DbSet<State> States { get; set; } = null!;
        public virtual DbSet<Store> Stores { get; set; } = null!;
        public virtual DbSet<StoreMoneyTransfer> StoreMoneyTransfers { get; set; } = null!;
        public virtual DbSet<StoreService> StoreServices { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;
        public virtual DbSet<UserSession> UserSessions { get; set; } = null!;
        public virtual DbSet<UserStoreMapping> UserStoreMappings { get; set; } = null!;
        public virtual DbSet<Wallet> Wallets { get; set; } = null!;
        public virtual DbSet<WalletType> WalletTypes { get; set; } = null!;
        public virtual DbSet<WalletTypeMapping> WalletTypeMappings { get; set; } = null!;
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
            modelBuilder.Entity<AggregatedCounter>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("PK_HangFire_CounterAggregated");

                entity.ToTable("AggregatedCounter", "HangFire");

                entity.HasIndex(e => e.ExpireAt, "IX_HangFire_AggregatedCounter_ExpireAt")
                    .HasFilter("([ExpireAt] IS NOT NULL)");

                entity.Property(e => e.Key).HasMaxLength(100);

                entity.Property(e => e.ExpireAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Counter>(entity =>
            {
                entity.HasKey(e => new { e.Key, e.Id })
                    .HasName("PK_HangFire_Counter");

                entity.ToTable("Counter", "HangFire");

                entity.Property(e => e.Key).HasMaxLength(100);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ExpireAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<CustomerMoneyTransfer>(entity =>
            {
                entity.ToTable("CustomerMoneyTransfer");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.CustomerMoneyTransfers)
                    .HasForeignKey(d => d.MarketZoneId)
                    .HasConstraintName("FK_CustomerMoneyTransfer_MarketZone");

                entity.HasOne(d => d.PackageItem)
                    .WithMany(p => p.CustomerMoneyTransfers)
                    .HasForeignKey(d => d.PackageItemId)
                    .HasConstraintName("FK_CustomerMoneyTransfer_PackageItem");

                entity.HasOne(d => d.Transaction)
                    .WithMany(p => p.CustomerMoneyTransfers)
                    .HasForeignKey(d => d.TransactionId)
                    .HasConstraintName("FK_CustomerMoneyTransfer_Transaction");
            });

            modelBuilder.Entity<Deposit>(entity =>
            {
                entity.ToTable("Deposit");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.PaymentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Deposits)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Deposit_Order");

                entity.HasOne(d => d.PackageItem)
                    .WithMany(p => p.Deposits)
                    .HasForeignKey(d => d.PackageItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Deposit_PackageItem");

                entity.HasOne(d => d.Wallet)
                    .WithMany(p => p.Deposits)
                    .HasForeignKey(d => d.WalletId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Deposit_Wallet");
            });

            modelBuilder.Entity<Hash>(entity =>
            {
                entity.HasKey(e => new { e.Key, e.Field })
                    .HasName("PK_HangFire_Hash");

                entity.ToTable("Hash", "HangFire");

                entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Hash_ExpireAt")
                    .HasFilter("([ExpireAt] IS NOT NULL)");

                entity.Property(e => e.Key).HasMaxLength(100);

                entity.Property(e => e.Field).HasMaxLength(100);
            });

            modelBuilder.Entity<IssueType>(entity =>
            {
                entity.ToTable("IssueType");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(200);
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.ToTable("Job", "HangFire");

                entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Job_ExpireAt")
                    .HasFilter("([ExpireAt] IS NOT NULL)");

                entity.HasIndex(e => e.StateName, "IX_HangFire_Job_StateName")
                    .HasFilter("([StateName] IS NOT NULL)");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ExpireAt).HasColumnType("datetime");

                entity.Property(e => e.StateName).HasMaxLength(20);
            });

            modelBuilder.Entity<JobParameter>(entity =>
            {
                entity.HasKey(e => new { e.JobId, e.Name })
                    .HasName("PK_HangFire_JobParameter");

                entity.ToTable("JobParameter", "HangFire");

                entity.Property(e => e.Name).HasMaxLength(40);

                entity.HasOne(d => d.Job)
                    .WithMany(p => p.JobParameters)
                    .HasForeignKey(d => d.JobId)
                    .HasConstraintName("FK_HangFire_JobParameter_Job");
            });

            modelBuilder.Entity<JobQueue>(entity =>
            {
                entity.HasKey(e => new { e.Queue, e.Id })
                    .HasName("PK_HangFire_JobQueue");

                entity.ToTable("JobQueue", "HangFire");

                entity.Property(e => e.Queue).HasMaxLength(50);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.FetchedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<List>(entity =>
            {
                entity.HasKey(e => new { e.Key, e.Id })
                    .HasName("PK_HangFire_List");

                entity.ToTable("List", "HangFire");

                entity.HasIndex(e => e.ExpireAt, "IX_HangFire_List_ExpireAt")
                    .HasFilter("([ExpireAt] IS NOT NULL)");

                entity.Property(e => e.Key).HasMaxLength(100);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ExpireAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<MarketZone>(entity =>
            {
                entity.ToTable("MarketZone");

                entity.HasIndex(e => e.Address, "IX_MarketZone_Address")
                    .IsUnique();

                entity.HasIndex(e => e.Email, "IX_MarketZone_Email")
                    .IsUnique();

                entity.HasIndex(e => e.Location, "IX_MarketZone_Location")
                    .IsUnique();

                entity.HasIndex(e => e.PhoneNumber, "IX_MarketZone_PhoneNumber")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Location).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ShortName).HasMaxLength(50);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<MarketZoneConfig>(entity =>
            {
                entity.ToTable("MarketZoneConfig");

                entity.HasIndex(e => e.MarketZoneId, "IX_MarketZone_Config")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.MarketZone)
                    .WithOne(p => p.MarketZoneConfig)
                    .HasForeignKey<MarketZoneConfig>(d => d.MarketZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MarketZone_Config_MarketZone");
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("Menu");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Menus)
                    .HasForeignKey(d => d.StoreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Store_Menu_Store");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.InvoiceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.PaymentType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status).HasMaxLength(20);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.PackageId)
                    .HasConstraintName("FK_Order_Package");

                entity.HasOne(d => d.PackageItem)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.PackageItemId)
                    .HasConstraintName("FK_Order_PackageItem");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_Order_Store");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Order_User");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.Property(e => e.Vatamount).HasColumnName("VATAmount");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Order_Detail_Order");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_OrderDetail_Product");

                entity.HasOne(d => d.StoreService)
                    .WithMany(p => p.OrderDetails)
                    .HasForeignKey(d => d.StoreServiceId)
                    .HasConstraintName("FK_Order_Detail_Store_Service");
            });

            modelBuilder.Entity<Package>(entity =>
            {
                entity.ToTable("Package");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.PackageType)
                    .WithMany(p => p.Packages)
                    .HasForeignKey(d => d.PackageTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Package_PackageType");
            });

            modelBuilder.Entity<PackageDetail>(entity =>
            {
                entity.ToTable("PackageDetail");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.PackageDetails)
                    .HasForeignKey(d => d.PackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PackageDetail_Package");

                entity.HasOne(d => d.WalletType)
                    .WithMany(p => p.PackageDetails)
                    .HasForeignKey(d => d.WalletTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PackageDetail_WalletType");
            });

            modelBuilder.Entity<PackageItem>(entity =>
            {
                entity.ToTable("PackageItem");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Cccdpassport)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .HasColumnName("CCCDPassport");

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.Gender)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Rfid)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("RFID");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.PackageItems)
                    .HasForeignKey(d => d.PackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Package_Item_Package");

                entity.HasOne(d => d.Wallet)
                    .WithMany(p => p.PackageItems)
                    .HasForeignKey(d => d.WalletId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PackageItem_Wallet");
            });

            modelBuilder.Entity<PackageOrder>(entity =>
            {
                entity.ToTable("PackageOrder");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.CusCccdpassport)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .HasColumnName("CusCCCDPassport");

                entity.Property(e => e.CusEmail)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CusName).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.PackageOrders)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PackageOrder_Order");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.PackageOrders)
                    .HasForeignKey(d => d.PackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PackageOrder_Package");
            });

            modelBuilder.Entity<PackageType>(entity =>
            {
                entity.ToTable("PackageType");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.Zone)
                    .WithMany(p => p.PackageTypes)
                    .HasForeignKey(d => d.ZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PackageType_Zone");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.MenuId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_Menu");

                entity.HasOne(d => d.ProductCategory)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_ProductCategory");
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("ProductCategory");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.ToTable("Promotion");

                entity.HasIndex(e => e.PromotionCode, "IX_Promotion")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.PromotionCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.Promotions)
                    .HasForeignKey(d => d.MarketZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Promotion_MarketZone");
            });

            modelBuilder.Entity<PromotionOrder>(entity =>
            {
                entity.ToTable("PromotionOrder");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.PromotionOrders)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Promotion_Order_Order");

                entity.HasOne(d => d.Promotion)
                    .WithMany(p => p.PromotionOrders)
                    .HasForeignKey(d => d.PromotionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Promotion_Order_Promotion");
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("Report");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Solution).HasMaxLength(500);

                entity.Property(e => e.SolveBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.IssueType)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.IssueTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Report_Issue_Type");

                entity.HasOne(d => d.PackageItem)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.PackageItemId)
                    .HasConstraintName("FK_Report_PackageItem");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Report_User");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Schema>(entity =>
            {
                entity.HasKey(e => e.Version)
                    .HasName("PK_HangFire_Schema");

                entity.ToTable("Schema", "HangFire");

                entity.Property(e => e.Version).ValueGeneratedNever();
            });

            modelBuilder.Entity<Server>(entity =>
            {
                entity.ToTable("Server", "HangFire");

                entity.HasIndex(e => e.LastHeartbeat, "IX_HangFire_Server_LastHeartbeat");

                entity.Property(e => e.Id).HasMaxLength(200);

                entity.Property(e => e.LastHeartbeat).HasColumnType("datetime");
            });

            modelBuilder.Entity<Set>(entity =>
            {
                entity.HasKey(e => new { e.Key, e.Value })
                    .HasName("PK_HangFire_Set");

                entity.ToTable("Set", "HangFire");

                entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Set_ExpireAt")
                    .HasFilter("([ExpireAt] IS NOT NULL)");

                entity.HasIndex(e => new { e.Key, e.Score }, "IX_HangFire_Set_Score");

                entity.Property(e => e.Key).HasMaxLength(100);

                entity.Property(e => e.Value).HasMaxLength(256);

                entity.Property(e => e.ExpireAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<State>(entity =>
            {
                entity.HasKey(e => new { e.JobId, e.Id })
                    .HasName("PK_HangFire_State");

                entity.ToTable("State", "HangFire");

                entity.HasIndex(e => e.CreatedAt, "IX_HangFire_State_CreatedAt");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(20);

                entity.Property(e => e.Reason).HasMaxLength(100);

                entity.HasOne(d => d.Job)
                    .WithMany(p => p.States)
                    .HasForeignKey(d => d.JobId)
                    .HasConstraintName("FK_HangFire_State_Job");
            });

            modelBuilder.Entity<Store>(entity =>
            {
                entity.ToTable("Store");

                entity.HasIndex(e => e.Address, "IX_Store_Address")
                    .IsUnique();

                entity.HasIndex(e => e.Email, "IX_Store_Email")
                    .IsUnique();

                entity.HasIndex(e => e.PhoneNumber, "IX_Store_PhoneNumber")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(100);

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ShortName).HasMaxLength(30);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.Zone)
                    .WithMany(p => p.Stores)
                    .HasForeignKey(d => d.ZoneId)
                    .HasConstraintName("FK_Store_Zone");
            });

            modelBuilder.Entity<StoreMoneyTransfer>(entity =>
            {
                entity.ToTable("StoreMoneyTransfer");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.StoreMoneyTransfers)
                    .HasForeignKey(d => d.MarketZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StoreMoneyTransfer_MarketZone");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.StoreMoneyTransfers)
                    .HasForeignKey(d => d.StoreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StoreMoneyTransfer_Store");

                entity.HasOne(d => d.Transaction)
                    .WithMany(p => p.StoreMoneyTransfers)
                    .HasForeignKey(d => d.TransactionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StoreMoneyTransfer_Transaction");
            });

            modelBuilder.Entity<StoreService>(entity =>
            {
                entity.ToTable("StoreService");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.StoreServices)
                    .HasForeignKey(d => d.StoreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Store_Service_Store");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transaction");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

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

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_Transaction_Order");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_Transaction_Store");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Transaction_User");

                entity.HasOne(d => d.Wallet)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.WalletId)
                    .HasConstraintName("FK_Transaction_Wallet");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.CccdPassport, "IX_User_CCCD_Passport")
                    .IsUnique();

                entity.HasIndex(e => e.Email, "IX_User_Email")
                    .IsUnique();

                entity.HasIndex(e => e.PhoneNumber, "IX_User_PhoneNumber")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(200);

                entity.Property(e => e.Birthday).HasColumnType("date");

                entity.Property(e => e.CccdPassport)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .HasColumnName("CCCD_Passport");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.Description).HasMaxLength(400);

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
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
                    .HasColumnName("upsDate");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.MarketZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_MarketZone");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Role");
            });

            modelBuilder.Entity<UserRefreshToken>(entity =>
            {
                entity.ToTable("UserRefreshToken");

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

            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.ToTable("UserSession");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserSessions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Session_User");

                entity.HasOne(d => d.Zone)
                    .WithMany(p => p.UserSessions)
                    .HasForeignKey(d => d.ZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserSession_Zone");
            });

            modelBuilder.Entity<UserStoreMapping>(entity =>
            {
                entity.ToTable("UserStoreMapping");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.UserStoreMappings)
                    .HasForeignKey(d => d.StoreId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserStoreMapping_Store");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserStoreMappings)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserStoreMapping_User");
            });

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.ToTable("Wallet");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.Wallets)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_Wallet_Store");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Wallets)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_Wallet_User");

                entity.HasOne(d => d.WalletType)
                    .WithMany(p => p.Wallets)
                    .HasForeignKey(d => d.WalletTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Wallet_Wallet_Type");
            });

            modelBuilder.Entity<WalletType>(entity =>
            {
                entity.ToTable("WalletType");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

                entity.HasOne(d => d.MarketZone)
                    .WithMany(p => p.WalletTypes)
                    .HasForeignKey(d => d.MarketZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WalletType_MarketZone");
            });

            modelBuilder.Entity<WalletTypeMapping>(entity =>
            {
                entity.ToTable("WalletTypeMapping");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.ProductCategory)
                    .WithMany(p => p.WalletTypeMappings)
                    .HasForeignKey(d => d.ProductCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WalletTypeMapping_ProductCategory");

                entity.HasOne(d => d.WalletType)
                    .WithMany(p => p.WalletTypeMappings)
                    .HasForeignKey(d => d.WalletTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WalletTypeMapping_WalletType");
            });

            modelBuilder.Entity<WalletTypeStoreServiceMapping>(entity =>
            {
                entity.ToTable("WalletTypeStoreServiceMapping");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.HasOne(d => d.StoreService)
                    .WithMany(p => p.WalletTypeStoreServiceMappings)
                    .HasForeignKey(d => d.StoreServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StoreService_WalletType_Mapping_Store_Service");

                entity.HasOne(d => d.WalletType)
                    .WithMany(p => p.WalletTypeStoreServiceMappings)
                    .HasForeignKey(d => d.WalletTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StoreService_WalletType_Mapping_Wallet_Type");
            });

            modelBuilder.Entity<Zone>(entity =>
            {
                entity.ToTable("Zone");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate).HasColumnType("datetime");

                entity.Property(e => e.Location).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.UpsDate).HasColumnType("datetime");

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
