using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateSystem.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int OfferId { get; set; }
        public Offer Offer { get; set; }

        public int BuyerId { get; set; }
        public Buyer Buyer { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public PaymentType PaymentType { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [MaxLength(100)]
        public string TransactionId { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [MaxLength(250)]
        public string ReceiptUrl { get; set; }
    }
}
