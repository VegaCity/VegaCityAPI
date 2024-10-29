﻿using System;
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
        public virtual DbSet<Deposit> Deposits { get; set; } = null!;
        public virtual DbSet<DisputeReport> DisputeReports { get; set; } = null!;
        public virtual DbSet<Etag> Etags { get; set; } = null!;
        public virtual DbSet<EtagDetail> EtagDetails { get; set; } = null!;
        public virtual DbSet<EtagType> EtagTypes { get; set; } = null!;
        public virtual DbSet<Hash> Hashes { get; set; } = null!;
        public virtual DbSet<House> Houses { get; set; } = null!;
        public virtual DbSet<IssueType> IssueTypes { get; set; } = null!;
        public virtual DbSet<Job> Jobs { get; set; } = null!;
        public virtual DbSet<JobParameter> JobParameters { get; set; } = null!;
        public virtual DbSet<JobQueue> JobQueues { get; set; } = null!;
        public virtual DbSet<List> Lists { get; set; } = null!;
        public virtual DbSet<MarketZone> MarketZones { get; set; } = null!;
        public virtual DbSet<Menu> Menus { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public virtual DbSet<Package> Packages { get; set; } = null!;
        public virtual DbSet<PackageETagTypeMapping> PackageETagTypeMappings { get; set; } = null!;
        public virtual DbSet<ProductCategory> ProductCategories { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Schema> Schemas { get; set; } = null!;
        public virtual DbSet<Server> Servers { get; set; } = null!;
        public virtual DbSet<Set> Sets { get; set; } = null!;
        public virtual DbSet<State> States { get; set; } = null!;
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

                entity.HasIndex(e => e.Creator, "IX_DisputeReports_UserId");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Creator)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.SolveBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SolveDate).HasColumnType("datetime");

                entity.HasOne(d => d.IssueType)
                    .WithMany(p => p.DisputeReports)
                    .HasForeignKey(d => d.IssueTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DisputeReports_IssueType");

                entity.HasOne(d => d.Store)
                    .WithMany(p => p.DisputeReports)
                    .HasForeignKey(d => d.StoreId)
                    .HasConstraintName("FK_DisputeReports_Store");
            });

            modelBuilder.Entity<Etag>(entity =>
            {
                entity.ToTable("ETag");

                entity.HasIndex(e => e.EtagTypeId, "IX_ETag_ETagTypeId");

                entity.HasIndex(e => e.EtagCode, "IX_ETag_EtagCode")
                    .IsUnique();

                entity.HasIndex(e => e.MarketZoneId, "IX_ETag_MarketZoneId");

                entity.HasIndex(e => e.WalletId, "IX_ETag_WalletId")
                    .IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.EtagCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EtagTypeId).HasColumnName("ETagTypeId");

                entity.Property(e => e.ImageUrl).IsUnicode(false);

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
                    .WithOne(p => p.Etag)
                    .HasForeignKey<Etag>(d => d.WalletId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ETag_Wallet");
            });

            modelBuilder.Entity<EtagDetail>(entity =>
            {
                entity.ToTable("EtagDetail");

                entity.HasIndex(e => e.EtagId, "IX_EtagDetail_EtagId")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Birthday).HasColumnType("date");

                entity.Property(e => e.CccdPassport)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .HasColumnName("CCCD_Passport");

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.FullName).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

                entity.HasOne(d => d.Etag)
                    .WithOne(p => p.EtagDetail)
                    .HasForeignKey<EtagDetail>(d => d.EtagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EtagDetail_ETag");
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

            modelBuilder.Entity<House>(entity =>
            {
                entity.ToTable("House");

                entity.HasIndex(e => e.Address, "IX_House_Address")
                    .IsUnique();

                entity.HasIndex(e => e.Location, "IX_House_Location")
                    .IsUnique();

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

            modelBuilder.Entity<IssueType>(entity =>
            {
                entity.ToTable("IssueType");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

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

                entity.HasIndex(e => e.PhoneNumber, "IX_MarketZone_PhoneNumber")
                    .IsUnique();

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

                entity.HasIndex(e => e.OrderId, "IX_OrderDetail_OrderId");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CrDate)
                    .HasColumnType("datetime")
                    .HasColumnName("crDate");

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

                entity.Property(e => e.Vatrate).HasColumnName("VATRate");

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

                entity.HasIndex(e => e.Name, "IX_Role_Name")
                    .IsUnique();

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

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

                entity.HasIndex(e => e.Email, "IX_Store_Email");

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

                entity.Property(e => e.UpsDate)
                    .HasColumnType("datetime")
                    .HasColumnName("upsDate");

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

                entity.HasIndex(e => e.CccdPassport, "IX_User_CCCD_Passport")
                    .IsUnique();

                entity.HasIndex(e => e.Email, "IX_User_Email")
                    .IsUnique();

                entity.HasIndex(e => e.PhoneNumber, "IX_User_PhoneNumber")
                    .IsUnique();

                entity.HasIndex(e => e.RoleId, "IX_User_RoleId");

                entity.HasIndex(e => e.StoreId, "IX_User_StoreId");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Address).HasMaxLength(200);

                entity.Property(e => e.Birthday).HasColumnType("date");

                entity.Property(e => e.CccdPassport)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .HasColumnName("CCCD_Passport");

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
