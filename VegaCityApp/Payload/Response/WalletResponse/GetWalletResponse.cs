namespace VegaCityApp.API.Payload.Response.WalletResponse
{
    public class GetWalletResponse : ResponseAPI // add more
    {
        public Guid Id { get; set; }
        public Guid? WalletTypeId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public int Balance { get; set; }
        public int BalanceHistory { get; set; }
        public bool Deflag { get; set; }
        public Guid? UserId { get; set; }
        public Guid? StoreId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpireDate { get; set; }
    }

}
