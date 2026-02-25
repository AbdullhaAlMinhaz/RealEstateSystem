using System;

namespace RealEstateSystem.ViewModels
{
    public class PaymentSuccessViewModel
    {
        public int InvoiceId { get; set; }
        public string PropertyTitle { get; set; }

        public string TransactionId { get; set; }
        public decimal AmountPaid { get; set; }
        public string StatusText { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Message { get; set; }
    }
}
