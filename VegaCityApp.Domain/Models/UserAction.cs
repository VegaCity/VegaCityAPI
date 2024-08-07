using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class UserAction
    {
        public Guid Id { get; set; }
        public Guid? UserActionTypeId { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? UserWalletId { get; set; }
        public Guid? MarketZoneCardId { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }

        public virtual MarketZoneCard? MarketZoneCard { get; set; }
        public virtual Order? Order { get; set; }
        public virtual Store? Store { get; set; }
        public virtual UserActionType? UserActionType { get; set; }
        public virtual UserWallet? UserWallet { get; set; }
    }
}
