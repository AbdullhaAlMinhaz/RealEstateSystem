using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.Models
{
    public class Inquiry
    {
        [Key]
        public int InquiryId { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public int BuyerId { get; set; }
        public Buyer Buyer { get; set; }

        public int SellerId { get; set; }
        public Seller Seller { get; set; }

        [MaxLength(200)]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime InquiryDate { get; set; } = DateTime.Now;

        public InquiryStatus InquiryStatus { get; set; } = InquiryStatus.New;

        public bool IsRead { get; set; } = false;

        public string ResponseMessage { get; set; }

        public DateTime? ResponseDate { get; set; }
    }
}
