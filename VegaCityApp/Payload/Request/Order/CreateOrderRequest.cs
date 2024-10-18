﻿using VegaCityApp.API.Payload.Request.Etag;

namespace VegaCityApp.API.Payload.Request.Order
{
    public class CreateOrderRequest
    {
        public string OrderName { get; set; }
        public string PaymentType { get; set; }
        public string SaleType { get; set; }
        public Guid StoreId { get; set; }
        public int TotalAmount { get; set; }
        public Guid MenuId { get; set; }
        public List<OrderProductFromPosRequest> ProductData { get; set; }
        public string InvoiceId { get; set; }
        public string EtagCode { get; set; }
    }
    public class CreateOrderForCashierRequest
    {
        public string SaleType { get; set; }
        public string PaymentType { get; set; }
        public int TotalAmount { get; set; }
        public List<OrderProductFromCashierRequest> ProductData { get; set; }
        public CustomerInfo CustomerInfo { get; set; }

    }
    public class ConfirmOrderForCashierRequest
    {
        public string InvoiceId { get; set; }
        public GenerateEtagRequest? GenerateEtagRequest { get; set; }
    }
    public class CustomerInfo
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string CCCD { get; set; }
    }
}
