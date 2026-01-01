using Microsoft.AspNetCore.Http;
using RealEstateSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.ViewModels
{
    public class SellerCommissionPaymentViewModel
    {
        public int CommissionInvoiceId { get; set; }

        public string PropertyTitle { get; set; }

        public decimal ListingPrice { get; set; }

        [Range(2, 5)]
        public int CommissionRatePercent { get; set; } = 2;

        public decimal CommissionAmount { get; set; }

        public CommissionInvoiceStatus Status { get; set; }

        [Required]
        public CommissionPaymentMethod PaymentMethod { get; set; }

        [Required]
        public string TransactionId { get; set; }

        [Required]
        public IFormFile ProofImage { get; set; }
    }
}
