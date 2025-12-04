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

        [Column(TypeName = "decimal(18,2)")]
        public decimal OfferAmount { get; set; }

        public DateTime OfferDate { get; set; }

        public OfferStatus OfferStatus { get; set; }

        public DateTime? ResponseDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CounterOfferAmount { get; set; }

        // These are required in the DB – make sure they are never null
        public string BuyerNotes { get; set; } = string.Empty;

        public string SellerNotes { get; set; } = string.Empty;

        public DateTime? ExpiryDate { get; set; }

        // Navigation
        public Payment Payment { get; set; }
    }
}
