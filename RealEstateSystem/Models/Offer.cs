using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateSystem.Models
{
    public class Offer
    {
        [Key]
        public int OfferId { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public int BuyerId { get; set; }
        public Buyer Buyer { get; set; }

        public int SellerId { get; set; }
        public Seller Seller { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OfferAmount { get; set; }

        public OfferStatus OfferStatus { get; set; } = OfferStatus.Pending;

        public DateTime OfferDate { get; set; } = DateTime.Now;

        public DateTime? ResponseDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CounterOfferAmount { get; set; }

        public string BuyerNotes { get; set; }

        public string SellerNotes { get; set; }

        public DateTime? ExpiryDate { get; set; }

        // Navigation
        public Payment Payment { get; set; }
    }
}
