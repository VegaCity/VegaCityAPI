namespace VegaCityApp.API.Payload.Response.UserResponse
{
    public class GetDepositApprovalResponse
    {
        public Guid TransactionId { get; set; }
        public string TypeTransaction { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int Balance { get; set; }
        public int BalanceHistory { get; set; }
    }
}
