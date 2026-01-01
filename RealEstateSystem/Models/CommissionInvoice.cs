using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateSystem.Models
{
    public class CommissionInvoice
    {
        public int CommissionInvoiceId { get; set; }

        // (FK)
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public int SellerId { get; set; }
        public Seller Seller { get; set; }

        // Invoice values
        [Column(TypeName = "decimal(18,2)")]
        public decimal ListingPrice { get; set; }

        // Only 2% to 5% (we’ll use dropdown later)
        public int CommissionRatePercent { get; set; } = 2;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; }

        public CommissionInvoiceStatus Status { get; set; } = CommissionInvoiceStatus.Unpaid;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // (Step 3 will use these fields)
        public CommissionPaymentMethod? PaymentMethod { get; set; }
        public string TransactionId { get; set; } = "";
        public string ProofImageUrl { get; set; } = "";

        public DateTime? SubmittedDate { get; set; }

        // (Step 4 will use these fields)
        public string AdminNote { get; set; } = "";
        public DateTime? VerifiedDate { get; set; }


        public string GatewayTranId { get; set; } = "";
        public string GatewayValId { get; set; } = "";
        public string GatewayStatus { get; set; } = "";

    }

    public enum CommissionInvoiceStatus
    {
        Unpaid = 0,
        PendingVerification = 1,
        Paid = 2,
        Rejected = 3
    }

    public enum CommissionPaymentMethod
    {
        Bank = 0,
        bKash = 1,
        Nagad = 2,
        Rocket = 3,
        Cash = 4,
        Other = 5
    }
}
