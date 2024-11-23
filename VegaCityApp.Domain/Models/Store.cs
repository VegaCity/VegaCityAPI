using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Store
    {
        public Store()
        {
            Menus = new HashSet<Menu>();
            Orders = new HashSet<Order>();
            ProductCategories = new HashSet<ProductCategory>();
            Reports = new HashSet<Report>();
            StoreMoneyTransfers = new HashSet<StoreMoneyTransfer>();
            Transactions = new HashSet<Transaction>();
            UserStoreMappings = new HashSet<UserStoreMapping>();
            Wallets = new HashSet<Wallet>();
        }

        public Guid Id { get; set; }
        public int? StoreType { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string? ShortName { get; set; }
        public string Email { get; set; } = null!;
        public Guid? ZoneId { get; set; }
        public Guid MarketZoneId { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }

        public virtual Zone? Zone { get; set; }
        public virtual ICollection<Menu> Menus { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ProductCategory> ProductCategories { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<StoreMoneyTransfer> StoreMoneyTransfers { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<UserStoreMapping> UserStoreMappings { get; set; }
        public virtual ICollection<Wallet> Wallets { get; set; }
    }
}
